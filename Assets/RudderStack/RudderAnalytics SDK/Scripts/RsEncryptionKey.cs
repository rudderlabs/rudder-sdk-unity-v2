using System;
using UnityEngine;

public class RsEncryptionKey : ScriptableObject
{
    public string key;

    [ContextMenu("Regenerate Key")]
    public void GenerateKey()
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        key = Convert.ToBase64String(aes.Key);
    }
}