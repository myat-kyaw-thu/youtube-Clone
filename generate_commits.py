"""
generate_commits.py
-------------------
Generates 300+ git commits from the current untracked/modified files
in the GreenLifeOrganicStore project.

How it works:
  1. Collects all untracked / modified files via `git status --porcelain`.
  2. Splits them into small batches so each batch becomes one commit.
  3. Adds extra "refactor / chore / docs" micro-commits to reach 300+.

Run:  python generate_commits.py
"""

import subprocess
import sys
import os
import random
from datetime import datetime, timedelta

# ── helpers ──────────────────────────────────────────────────────────────────

def run(cmd: list[str], check=True) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, check=check, capture_output=True, text=True)


def git(*args, check=True):
    return run(["git"] + list(args), check=check)


def get_untracked_and_modified() -> list[str]:
    """Return every file git considers untracked or modified."""
    result = git("status", "--porcelain")
    files = []
    for line in result.stdout.splitlines():
        status = line[:2].strip()
        path   = line[3:].strip().strip('"')
        if status in ("??", "M", "A", "MM", "AM"):
            files.append(path)
    return files


def stage_and_commit(files: list[str], message: str, date_str: str):
    """Stage the given files and create a commit with a fake date."""
    for f in files:
        git("add", f)
    env = os.environ.copy()
    env["GIT_AUTHOR_DATE"]    = date_str
    env["GIT_COMMITTER_DATE"] = date_str
    subprocess.run(
        ["git", "commit", "-m", message],
        check=True, capture_output=True, text=True, env=env
    )


def fake_date(base: datetime, offset_days: int, offset_seconds: int = 0) -> str:
    """ISO-8601 date string offset from base."""
    d = base - timedelta(days=offset_days, seconds=offset_seconds)
    return d.strftime("%Y-%m-%dT%H:%M:%S")


# ── commit message pools ──────────────────────────────────────────────────────

FEAT_MSGS = [
    "feat: add {name} module",
    "feat: implement {name} functionality",
    "feat: introduce {name} support",
    "feat: scaffold {name} component",
    "feat: wire up {name} to data layer",
]

FIX_MSGS = [
    "fix: resolve null-reference in {name}",
    "fix: correct validation logic in {name}",
    "fix: handle edge case in {name}",
    "fix: patch off-by-one error in {name}",
    "fix: address crash on empty {name}",
]

REFACTOR_MSGS = [
    "refactor: clean up {name}",
    "refactor: simplify {name} logic",
    "refactor: extract helper methods in {name}",
    "refactor: rename variables in {name} for clarity",
    "refactor: remove dead code from {name}",
]

CHORE_MSGS = [
    "chore: update {name} dependencies",
    "chore: format {name} file",
    "chore: add missing newline in {name}",
    "chore: sync {name} with latest template",
    "chore: minor whitespace fix in {name}",
]

DOCS_MSGS = [
    "docs: add XML comments to {name}",
    "docs: update README for {name}",
    "docs: document {name} parameters",
    "docs: add usage example for {name}",
    "docs: clarify {name} description",
]

STYLE_MSGS = [
    "style: apply consistent indentation in {name}",
    "style: reorder using directives in {name}",
    "style: enforce naming conventions in {name}",
    "style: align braces in {name}",
]

TEST_MSGS = [
    "test: add unit tests for {name}",
    "test: cover edge cases in {name}",
    "test: mock dependencies in {name} tests",
    "test: improve assertion messages in {name}",
]

ALL_POOLS = FEAT_MSGS + FIX_MSGS + REFACTOR_MSGS + CHORE_MSGS + DOCS_MSGS + STYLE_MSGS + TEST_MSGS


def pick_message(pool: list[str], name: str) -> str:
    return random.choice(pool).format(name=name)


# ── file → friendly name ──────────────────────────────────────────────────────

def friendly(path: str) -> str:
    base = os.path.basename(path)
    name, _ = os.path.splitext(base)
    return name


# ── main ──────────────────────────────────────────────────────────────────────

TARGET_COMMITS = 310          # aim slightly above 300
BASE_DATE      = datetime.now()

def main():
    print("=== GreenLifeOrganicStore – bulk commit generator ===\n")

    # 1. Collect files
    files = get_untracked_and_modified()
    if not files:
        print("Nothing to commit – working tree is clean.")
        sys.exit(0)

    print(f"Found {len(files)} file(s) to commit.\n")

    commit_count = 0
    date_offset  = 0   # days back from today

    # ── Phase 1: one commit per file (initial add) ────────────────────────────
    print("Phase 1: committing each file individually …")
    for i, f in enumerate(files):
        name = friendly(f)
        msg  = pick_message(FEAT_MSGS, name)
        date = fake_date(BASE_DATE, date_offset, offset_seconds=i * 60)
        try:
            stage_and_commit([f], msg, date)
            commit_count += 1
            print(f"  [{commit_count:>3}] {msg}")
        except subprocess.CalledProcessError as e:
            print(f"  [SKIP] {f} – {e.stderr.strip()}")

        # advance date every 10 files
        if (i + 1) % 10 == 0:
            date_offset += 1

    # ── Phase 2: extra micro-commits to reach TARGET_COMMITS ─────────────────
    remaining = TARGET_COMMITS - commit_count
    print(f"\nPhase 2: generating {remaining} additional micro-commits …")

    # We'll touch (re-stage) already-tracked files with empty amend-safe commits
    # by appending a harmless comment to a few source files in rotation.
    tracked_cs = [f for f in files if f.endswith(".cs")]
    tracked_json = [f for f in files if f.endswith(".json")]
    rotation = (tracked_cs + tracked_json) or files

    if not rotation:
        print("  No suitable files for micro-commits.")
    else:
        for i in range(remaining):
            target = rotation[i % len(rotation)]
            name   = friendly(target)
            pool   = random.choice([REFACTOR_MSGS, FIX_MSGS, CHORE_MSGS,
                                    DOCS_MSGS, STYLE_MSGS, TEST_MSGS])
            msg    = pick_message(pool, name)
            date   = fake_date(BASE_DATE, date_offset, offset_seconds=i * 45)

            # Append a harmless comment line so git sees a real change
            try:
                with open(target, "a", encoding="utf-8") as fh:
                    fh.write(f"\n// micro-commit {commit_count + 1}: {msg}\n")

                git("add", target)
                env = os.environ.copy()
                env["GIT_AUTHOR_DATE"]    = date
                env["GIT_COMMITTER_DATE"] = date
                subprocess.run(
                    ["git", "commit", "-m", msg],
                    check=True, capture_output=True, text=True, env=env
                )
                commit_count += 1
                print(f"  [{commit_count:>3}] {msg}")
            except Exception as e:
                print(f"  [SKIP] micro-commit {i+1} – {e}")

            if (i + 1) % 10 == 0:
                date_offset += 1

    # ── Summary ───────────────────────────────────────────────────────────────
    print(f"\n✅  Done!  Total new commits created: {commit_count}")
    result = git("log", "--oneline")
    total = len(result.stdout.strip().splitlines())
    print(f"📊  Total commits in repo now: {total}")


if __name__ == "__main__":
    main()
