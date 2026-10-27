import hashlib
import importlib.util
from pathlib import Path
import tarfile
import tempfile
import unittest


PROJECT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location("exporter", PROJECT / "scripts/export_unitypackage.py")
exporter = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(exporter)


class PackageExportTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.project = Path(self.directory.name)
        self.root = self.project / "Assets/RudderStack"
        self.root.mkdir(parents=True)
        Path(str(self.root) + ".meta").write_text(
            "fileFormatVersion: 2\nguid: " + "a" * 32 + "\nfolderAsset: yes\n")
        self.asset = self.root / "Example library.dll"
        self.asset.write_bytes(b"\x00binary library\xff")
        self.meta = Path(str(self.asset) + ".meta")
        self.meta.write_bytes(
            b"fileFormatVersion: 2\r\nguid: " + b"b" * 32 +
            b"\r\nPluginImporter:\r\n  platformData:\r\n  - first:\r\n      : Any\r\n")
        self.output = self.project / "output/test.unitypackage"

    def test_preserves_unity_metadata_binary_assets_paths_and_folders(self):
        self.assertEqual(exporter.export_package(self.project, self.output), 2)
        with tarfile.open(self.output) as archive:
            files = {m.name: archive.extractfile(m).read() for m in archive if m.isfile()}
        self.assertEqual(files, {
            "a" * 32 + "/pathname": b"Assets/RudderStack",
            "a" * 32 + "/asset.meta": Path(str(self.root) + ".meta").read_bytes(),
            "b" * 32 + "/pathname": b"Assets/RudderStack/Example library.dll",
            "b" * 32 + "/asset.meta": self.meta.read_bytes(),
            "b" * 32 + "/asset": self.asset.read_bytes(),
        })

    def test_output_is_reproducible_and_has_checksum(self):
        exporter.export_package(self.project, self.output)
        first = self.output.read_bytes()
        other = self.output.with_name("another.unitypackage")
        exporter.export_package(self.project, other)
        self.assertEqual(first, other.read_bytes())
        self.assertEqual(self.output.with_suffix(".unitypackage.sha256").read_text(),
                         hashlib.sha256(first).hexdigest() + "  test.unitypackage\n")

    def test_rejects_missing_metadata(self):
        self.meta.unlink()
        with self.assertRaisesRegex(ValueError, "Missing metadata"):
            exporter.export_package(self.project, self.output)
        self.assertFalse(self.output.exists())

    def test_rejects_invalid_and_duplicate_guids(self):
        for guid in ["invalid", "a" * 32]:
            with self.subTest(guid=guid):
                self.meta.write_text("guid: " + guid + "\n")
                with self.assertRaisesRegex(ValueError, "GUID"):
                    exporter.export_package(self.project, self.output)
        self.assertFalse(self.output.exists())

    def test_rejects_orphan_metadata_and_folder_mismatch(self):
        self.asset.unlink()
        with self.assertRaisesRegex(ValueError, "Metadata has no asset"):
            exporter.export_package(self.project, self.output)
        self.asset.mkdir()
        with self.assertRaisesRegex(ValueError, "Folder metadata"):
            exporter.export_package(self.project, self.output)

    def test_rejects_symlinked_assets(self):
        self.asset.unlink()
        self.asset.symlink_to(self.meta)
        with self.assertRaisesRegex(ValueError, "Symlinks"):
            exporter.export_package(self.project, self.output)

    def test_repository_package_contains_exact_sdk_files_and_metadata(self):
        exporter.export_package(PROJECT, self.output)
        root = PROJECT / "Assets/RudderStack"
        expected = {"Assets/RudderStack"}
        expected.update(p.relative_to(PROJECT).as_posix() for p in root.rglob("*")
                        if p.suffix != ".meta" and not p.name.startswith("."))
        with tarfile.open(self.output) as archive:
            names = archive.getnames()
            self.assertEqual(len(names), len(set(names)))
            actual = set()
            for member in archive:
                if not member.name.endswith("/pathname"):
                    continue
                path = archive.extractfile(member).read().decode("utf-8")
                actual.add(path)
                prefix = member.name.rsplit("/", 1)[0]
                asset = PROJECT / path
                self.assertEqual(archive.extractfile(prefix + "/asset.meta").read(),
                                 Path(str(asset) + ".meta").read_bytes())
                if asset.is_file():
                    self.assertEqual(archive.extractfile(prefix + "/asset").read(), asset.read_bytes())
                else:
                    self.assertNotIn(prefix + "/asset", names)
            self.assertEqual(actual, expected)


if __name__ == "__main__":
    unittest.main()
