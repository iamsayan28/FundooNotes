using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interface
{
    public interface IEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string encryptedText);
    }
}
