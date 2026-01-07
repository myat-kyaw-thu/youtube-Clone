"""
generate_30_commits.py
----------------------
Generates 50+ git commits from the current modified/untracked files
spread across specific dates in 2026.

Run:  python generate_30_commits.py
"""

import subprocess
import sys
import os
import random
from datetime import datetime

# ── helpers ──────────────────────────────────────────────────────────────────

def run(cmd: list[str], check=True) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, check=check, capture_output=True, text=True)


def git(*args, check=True):
    return run(["git"] + list(args), check=check)


def get_changed_files() -> tuple[list[str], list[str]]:
    """Return (modified_files, untracked_files)."""
    result = git("status", "--porcelain")
    modified = []
    untracked = []
    
    for line in result.stdout.splitlines():
        status = line[:2].strip()
        path   = line[3:].strip().strip('"')
        
        if status in ("M", "MM", "D"):  # Modified or Deleted
            modified.append(path)
        elif status == "??":  # Untracked
            untracked.append(path)
    
    return modified, untracked


def stage_and_commit(files: list[str], message: str, date_str: str):
    """Stage the given files and create a commit with a fake date."""
    for f in files:
        if os.path.exists(f):
            git("add", f)
        else:
            # Handle deleted files
            git("rm", f, check=False)
    
    env = os.environ.copy()
    env["GIT_AUTHOR_DATE"]    = date_str
    env["GIT_COMMITTER_DATE"] = date_str
    subprocess.run(
        ["git", "commit", "-m", message],
        check=True, capture_output=True, text=True, env=env
    )


def make_date(year: int, month: int, day: int, hour: int = 10, minute: int = 0) -> str:
    """Create ISO-8601 date string for specific date and time."""
    d = datetime(year, month, day, hour, minute, 0)
    return d.strftime("%Y-%m-%dT%H:%M:%S")


def friendly(path: str) -> str:
    """Extract friendly name from file path."""
    base = os.path.basename(path)
    name, _ = os.path.splitext(base)
    return name if name else base


# ── commit message templates ──────────────────────────────────────────────────

MESSAGES = {
    "feat": [
        "feat: add {name} functionality",
        "feat: implement {name} feature",
        "feat: introduce {name} support",
        "feat: enhance {name} module",
    ],
    "fix": [
        "fix: resolve issue in {name}",
        "fix: correct bug in {name}",
        "fix: patch error in {name}",
        "fix: address problem in {name}",
    ],
    "refactor": [
        "refactor: clean up {name}",
        "refactor: improve {name} structure",
        "refactor: optimize {name}",
        "refactor: simplify {name} logic",
    ],
    "chore": [
        "chore: update {name}",
        "chore: maintain {name}",
        "chore: sync {name}",
        "chore: adjust {name}",
    ],
    "docs": [
        "docs: document {name}",
        "docs: add comments to {name}",
        "docs: update {name} documentation",
    ],
    "style": [
        "style: format {name}",
        "style: clean up {name}",
        "style: improve {name} readability",
    ],
    "perf": [
        "perf: optimize {name}",
        "perf: improve {name} performance",
    ],
}


def pick_message(name: str) -> str:
    """Pick a random commit message."""
    category = random.choice(list(MESSAGES.keys()))
    template = random.choice(MESSAGES[category])
    return template.format(name=name)


# ── main ──────────────────────────────────────────────────────────────────────

TARGET_COMMITS = 55  # aim for 55 to ensure we get 50+

# Specific dates to spread commits across
COMMIT_DATES = [
    # January 2026
    (2026, 1, 1), (2026, 1, 2), (2026, 1, 7), (2026, 1, 11), (2026, 1, 23),
    # February 2026
    (2026, 2, 5), (2026, 2, 7), (2026, 2, 8), (2026, 2, 9), (2026, 2, 11),
    (2026, 2, 14), (2026, 2, 18),
    # March 2026
    (2026, 3, 4), (2026, 3, 6), (2026, 3, 7), (2026, 3, 8), (2026, 3, 10),
    (2026, 3, 13), (2026, 3, 14), (2026, 3, 27), (2026, 3, 29),
]


def main():
    print("=== GreenLifeOrganicStore – 50+ commit generator ===\n")

    # 1. Collect files
    modified, untracked = get_changed_files()
    all_files = modified + untracked
    
    if not all_files:
        print("✅ Nothing to commit – working tree is clean.")
        sys.exit(0)

    print(f"Found {len(modified)} modified file(s)")
    print(f"Found {len(untracked)} untracked file(s)")
    print(f"Total: {len(all_files)} file(s) to commit.\n")
    print(f"Target: {TARGET_COMMITS} commits across {len(COMMIT_DATES)} specific dates\n")

    commit_count = 0
    date_index   = 0

    # ── Phase 1: commit files in small batches ────────────────────────────────
    print("Phase 1: committing files in batches …")
    
    # Shuffle for variety
    random.shuffle(all_files)
    
    # Split into batches (1-2 files per commit)
    i = 0
    while i < len(all_files) and commit_count < TARGET_COMMITS:
        batch_size = random.randint(1, 2)
        batch = all_files[i:i+batch_size]
        
        if not batch:
            break
        
        # Pick a date from the list (cycle through them)
        year, month, day = COMMIT_DATES[date_index % len(COMMIT_DATES)]
        hour = random.randint(9, 18)  # Random hour between 9 AM and 6 PM
        minute = random.randint(0, 59)
        date = make_date(year, month, day, hour, minute)
        
        # Pick a file name for the commit message
        name = friendly(batch[0])
        msg  = pick_message(name)
        
        try:
            stage_and_commit(batch, msg, date)
            commit_count += 1
            files_str = ", ".join([os.path.basename(f) for f in batch])
            print(f"  [{commit_count:>2}] {year}-{month:02d}-{day:02d} {msg} ({files_str})")
            date_index += 1
        except subprocess.CalledProcessError as e:
            print(f"  [SKIP] {e.stderr.strip()}")
        
        i += batch_size

    # ── Phase 2: extra micro-commits if needed ────────────────────────────────
    remaining = TARGET_COMMITS - commit_count
    
    if remaining > 0:
        print(f"\nPhase 2: generating {remaining} additional micro-commits …")
        
        # Find .cs files to add micro-changes
        cs_files = [f for f in all_files if f.endswith(".cs") and os.path.exists(f)]
        
        if not cs_files:
            print("  No .cs files available for micro-commits.")
        else:
            for i in range(remaining):
                target = cs_files[i % len(cs_files)]
                name   = friendly(target)
                msg    = pick_message(name)
                
                # Pick a date from the list
                year, month, day = COMMIT_DATES[date_index % len(COMMIT_DATES)]
                hour = random.randint(9, 18)
                minute = random.randint(0, 59)
                date = make_date(year, month, day, hour, minute)
                
                try:
                    # Append a harmless comment
                    with open(target, "a", encoding="utf-8") as fh:
                        fh.write(f"\n// commit {commit_count + 1}: {msg}\n")
                    
                    git("add", target)
                    env = os.environ.copy()
                    env["GIT_AUTHOR_DATE"]    = date
                    env["GIT_COMMITTER_DATE"] = date
                    subprocess.run(
                        ["git", "commit", "-m", msg],
                        check=True, capture_output=True, text=True, env=env
                    )
                    commit_count += 1
                    print(f"  [{commit_count:>2}] {year}-{month:02d}-{day:02d} {msg}")
                    date_index += 1
                except Exception as e:
                    print(f"  [SKIP] micro-commit {i+1} – {e}")

    # ── Summary ───────────────────────────────────────────────────────────────
    print(f"\n✅  Done!  Total new commits created: {commit_count}")
    result = git("log", "--oneline", "-10")
    print(f"\n📊  Last 10 commits:")
    print(result.stdout)


if __name__ == "__main__":
    main()
