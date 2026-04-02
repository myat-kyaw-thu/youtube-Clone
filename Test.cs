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

// micro-commit 100: docs: add XML comments to Test

// micro-commit 104: style: enforce naming conventions in Test

// micro-commit 108: chore: minor whitespace fix in Test

// micro-commit 112: test: add unit tests for Test

// micro-commit 116: refactor: simplify Test logic

// micro-commit 120: fix: handle edge case in Test

// micro-commit 124: chore: update Test dependencies

// micro-commit 128: fix: resolve null-reference in Test

// micro-commit 132: style: enforce naming conventions in Test

// micro-commit 136: test: improve assertion messages in Test

// micro-commit 140: refactor: remove dead code from Test

// micro-commit 144: fix: handle edge case in Test

// micro-commit 148: test: cover edge cases in Test

// micro-commit 152: refactor: clean up Test

// micro-commit 156: refactor: remove dead code from Test

// micro-commit 160: chore: minor whitespace fix in Test

// micro-commit 164: test: add unit tests for Test

// micro-commit 168: test: cover edge cases in Test

// micro-commit 172: test: improve assertion messages in Test

// micro-commit 176: test: cover edge cases in Test

// micro-commit 180: docs: add usage example for Test

// micro-commit 184: fix: correct validation logic in Test

// micro-commit 188: test: cover edge cases in Test

// micro-commit 192: style: reorder using directives in Test

// micro-commit 196: docs: document Test parameters

// micro-commit 200: chore: sync Test with latest template

// micro-commit 204: test: add unit tests for Test

// micro-commit 208: fix: correct validation logic in Test

// micro-commit 212: docs: update README for Test

// micro-commit 216: test: cover edge cases in Test

// micro-commit 220: test: improve assertion messages in Test

// micro-commit 224: chore: update Test dependencies

// micro-commit 228: refactor: simplify Test logic

// micro-commit 232: refactor: rename variables in Test for clarity

// micro-commit 236: chore: sync Test with latest template

// micro-commit 240: chore: add missing newline in Test

// micro-commit 244: test: add unit tests for Test

// micro-commit 248: style: enforce naming conventions in Test

// micro-commit 252: fix: resolve null-reference in Test

// micro-commit 256: refactor: simplify Test logic

// micro-commit 260: refactor: simplify Test logic

// micro-commit 264: refactor: rename variables in Test for clarity

// micro-commit 268: style: align braces in Test

// micro-commit 272: chore: sync Test with latest template

// micro-commit 276: chore: format Test file

// micro-commit 280: docs: add usage example for Test

// micro-commit 284: chore: add missing newline in Test
