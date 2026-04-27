using System;
using System.Security.Cryptography;
using System.Text;

class Test {
    static void Main() {
        string password = "password";
        byte[] saltBytes = new byte[32];
        using (var rng = new RNGCryptoServiceProvider()) {
            rng.GetBytes(saltBytes);
        }
        string salt = Convert.ToBase64String(saltBytes);
        string combined = password + salt;
        using (var sha256 = SHA256.Create()) {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            string hash = Convert.ToBase64String(hashBytes);
            Console.WriteLine(salt + ":" + hash);
        }
    }
}

// micro-commit 28: docs: clarify Test description

// micro-commit 32: refactor: remove dead code from Test
