#!/usr/bin/env python3
"""Package Assets/RudderStack without changing Unity's serialized metadata."""

import argparse
import gzip
import hashlib
import io
from pathlib import Path
import re
import tarfile


PROJECT = Path(__file__).resolve().parents[1]
ASSET_ROOT = Path("Assets/RudderStack")


def collect_assets(project):
    root = project / ASSET_ROOT
    if not root.is_dir():
        raise ValueError(f"Missing SDK directory: {root}")

    assets = [root] + sorted(
        path for path in root.rglob("*")
        if path.suffix != ".meta"
        and not any(part.startswith(".") for part in path.relative_to(root).parts)
    )
    records = []
    guids = set()
    for asset in assets:
        meta = Path(str(asset) + ".meta")
        if asset.is_symlink() or meta.is_symlink():
            raise ValueError(f"Symlinks cannot be packaged: {asset}")
        if not meta.is_file():
            raise ValueError(f"Missing metadata: {meta}")
        metadata = meta.read_bytes()
        # Unity emits empty YAML mapping keys that generic YAML parsers reject.
        # Only the top-level GUID is needed; preserve the entire metadata verbatim.
        matches = re.findall(rb"^guid: ([0-9a-f]{32})\r?$", metadata, re.MULTILINE)
        if len(matches) != 1:
            raise ValueError(f"Expected one valid GUID: {meta}")
        guid = matches[0].decode("ascii")
        if guid in guids:
            raise ValueError(f"Duplicate GUID {guid}: {meta}")
        guids.add(guid)
        folder = re.search(rb"^folderAsset: yes\r?$", metadata, re.MULTILINE) is not None
        if folder != asset.is_dir():
            raise ValueError(f"Folder metadata does not match asset: {meta}")
        records.append((guid, asset.relative_to(project).as_posix(), metadata,
                        None if folder else asset.read_bytes()))

    for meta in root.rglob("*.meta"):
        if not Path(str(meta)[:-5]).exists():
            raise ValueError(f"Metadata has no asset: {meta}")
    return records


def add_file(archive, name, data):
    entry = tarfile.TarInfo(name)
    entry.size = len(data)
    entry.mode = 0o644
    archive.addfile(entry, io.BytesIO(data))


def export_package(project, output):
    records = collect_assets(project)
    output.parent.mkdir(parents=True, exist_ok=True)
    # Fixed headers and ordering make repeated exports reproducible.
    with output.open("wb") as stream:
        with gzip.GzipFile(filename="", mode="wb", fileobj=stream, mtime=0) as compressed:
            with tarfile.open(fileobj=compressed, mode="w", format=tarfile.USTAR_FORMAT) as archive:
                for guid, path, metadata, data in records:
                    directory = tarfile.TarInfo(guid + "/")
                    directory.type = tarfile.DIRTYPE
                    directory.mode = 0o755
                    archive.addfile(directory)
                    add_file(archive, guid + "/pathname", path.encode("utf-8"))
                    add_file(archive, guid + "/asset.meta", metadata)
                    if data is not None:
                        add_file(archive, guid + "/asset", data)
    digest = hashlib.sha256(output.read_bytes()).hexdigest()
    output.with_suffix(output.suffix + ".sha256").write_text(
        f"{digest}  {output.name}\n", encoding="utf-8")
    return len(records)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path,
                        default=PROJECT / "artifacts/RudderStackUnity.unitypackage")
    args = parser.parse_args()
    count = export_package(PROJECT, args.output)
    print(f"Exported {count} assets to {args.output}")


if __name__ == "__main__":
    main()
