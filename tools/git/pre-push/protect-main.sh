#!/bin/sh
# Pre-push check: changes reach main only through pull requests (docs/standards/git.md §5, R-03).
# Git writes one line per pushed ref to stdin: <local ref> <local sha> <remote ref> <remote sha>.
# Checking the remote ref (not the current branch) also catches pushes like "git push origin HEAD:main".
status=0
while read -r _local_ref _local_sha remote_ref _remote_sha; do
  if [ "$remote_ref" = "refs/heads/main" ]; then
    echo "Direct pushes to main are not allowed; open a pull request (docs/standards/git.md section 5)." >&2
    status=1
  fi
done
exit "$status"
