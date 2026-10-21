# BatuSales contributor / coding-agent instructions

1. **Inspect before editing.** Fetch the latest default branch, recent commits,
   active PRs/issues and affected files. Never rely on a previous session's SHA.
2. Read `docs/STATUS.md` and the P0/P1/P2 product scope. Compare requested work
   to code and tests; do NOT recommit an existing feature or recreate a PR.
3. Research current supported technology versions and new platform changes for
   each substantive session. Log findings and choose additions only when useful.
4. Build a cohesive vertical slice. Preserve module conventions and avoid adding
   dependencies unrelated to the selected feature. Write tests for edge cases.
5. Fail closed on auth/tenant handling. Tenant comes from server-verified
   membership, not a trusted client-supplied ID alone. Do not skip authorization.
6. Keep AI tools least-privileged. Human approval for payment, deletion and
   outbound bulk messaging. No fabricated knowledge-base answers.
7. Create a unique feature branch and a scoped PR. Check diff and CI; do not merge
   an unverified branch or claim tests passed without evidence.
8. Update `docs/STATUS.md` with precise implemented vs pending capabilities,
   blockers, the commit/PR reference and one clear next task.
9. Report: starting HEAD, files touched, reason for change, tests executed and
   results, remaining risk, next priority. Do not report imagined features.
