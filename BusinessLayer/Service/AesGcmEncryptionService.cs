using BusinessLayer.Interface;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Service
{
    public class AesGcmEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const string Prefix = "v1:";
        public AesGcmEncryptionService(IConfiguration configuration)
        {
            var keyBase64 = configuration["Encryption:Key"];

            if (string.IsNullOrWhiteSpace(keyBase64))
            {
                throw new InvalidOperationException("Encryption key is not configured.");
            }

            try
            {
                _key = Convert.FromBase64String(keyBase64);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Encryption key must be valid Base64.", ex);
            }

            if (_key.Length != 32)
            {
                throw new InvalidOperationException("AES-256 requires a 32-byte key.");
            }
        }

        public string Encrypt(string plaintext)
        {
            ArgumentNullException.ThrowIfNull(plaintext);

            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aes = new AesGcm(_key, TagSize);

            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // Stored format: version:Base64(nonce + tag + ciphertext)
            byte[] payload = new byte[nonce.Length + tag.Length + ciphertext.Length];

            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, payload,nonce.Length + tag.Length, ciphertext.Length);

            return Prefix + Convert.ToBase64String(payload);
        }

        public string Decrypt(string encryptedText)
        {
            ArgumentNullException.ThrowIfNull(encryptedText);

            if (!encryptedText.StartsWith(Prefix, StringComparison.Ordinal))
            {
                //throw new CryptographicException("Unsupported encrypted data format.");

                // to mitigate and return old un-encrypted text
                return encryptedText;
            }

            byte[] payload;

            try
            {
                payload = Convert.FromBase64String(encryptedText[Prefix.Length..]);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Invalid encrypted data format.", ex);
            }

            if (payload.Length < NonceSize + TagSize)
            {
                throw new CryptographicException("Encrypted data is incomplete.");
            }

            byte[] nonce = payload[..NonceSize];
            byte[] tag = payload[NonceSize..(NonceSize + TagSize)];
            byte[] ciphertext = payload[(NonceSize + TagSize)..];
            byte[] plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_key, TagSize);

            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
