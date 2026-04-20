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

// micro-commit 36: refactor: simplify Test logic

// micro-commit 40: refactor: remove dead code from Test

// micro-commit 44: chore: format Test file

// micro-commit 48: test: mock dependencies in Test tests

// micro-commit 52: style: align braces in Test

// micro-commit 56: chore: update Test dependencies

// micro-commit 60: chore: minor whitespace fix in Test

// micro-commit 64: fix: correct validation logic in Test

// micro-commit 68: style: align braces in Test

// micro-commit 72: chore: sync Test with latest template

// micro-commit 76: fix: patch off-by-one error in Test

// micro-commit 80: refactor: rename variables in Test for clarity

// micro-commit 84: refactor: simplify Test logic

// micro-commit 88: chore: sync Test with latest template

// micro-commit 92: fix: patch off-by-one error in Test

// micro-commit 96: test: mock dependencies in Test tests
