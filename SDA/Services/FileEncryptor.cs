using System.Security.Cryptography;
using System.Text;

namespace SDA.Services
{
    public static class FileEncryptor
    {
        public static string Encrypt(string data, string passkey)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passkey, salt, 10000);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }

                    byte[] encrypted = msEncrypt.ToArray();
                    byte[] result = new byte[salt.Length + encrypted.Length];
                    Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                    Buffer.BlockCopy(encrypted, 0, result, salt.Length, encrypted.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        public static SteamAuth.SteamGuardAccount Decrypt(string encryptedData, string passkey)
        {
            byte[] data = Convert.FromBase64String(encryptedData);
            byte[] salt = new byte[16];
            byte[] encrypted = new byte[data.Length - 16];

            Buffer.BlockCopy(data, 0, salt, 0, 16);
            Buffer.BlockCopy(data, 16, encrypted, 0, encrypted.Length);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passkey, salt, 10000);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(encrypted))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    string decrypted = srDecrypt.ReadToEnd();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<SteamAuth.SteamGuardAccount>(decrypted);
                }
            }
        }

        public static bool VerifyPasskey(string encryptedData, string passkey)
        {
            try
            {
                Decrypt(encryptedData, passkey);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 