# BatuSales contributor workflow

Before any change:
1. Read README.md, docs/STATUS.md and docs/TECH_RADAR.md.
2. Fetch the latest default-branch HEAD, open PRs, recent commits, and target files.
3. Compare the requested feature against existing implementations and PR diffs.
4. Do not repeat an existing commit, create a duplicate PR, or rewrite implemented work.
5. Select the smallest useful P0 vertical slice; use an isolated feature branch.

For every change:
1. Confirm the module already exists or add it consistently with the modular-monolith architecture.
2. Keep tenant authorization server-side. Never use a client-supplied tenant ID alone.
3. Test cross-tenant access, invalid inputs and errors. Do not claim tests passed unless observed.
4. Check official releases and security advisories for relevant dependencies. Prefer LTS and keep a short decision in docs/TECH_RADAR.md.
5. Avoid speculative technology additions, unnecessary packages or premature microservices.
6. Update docs/STATUS.md with shipped and remaining work, commit/PR references when known.
7. Commit only actual changes, inspect diff, and use a PR. Never force-push main.

Security release gates:
- No production rollout without migrations, email verification, TLS, backup/restore,
  rate-limit review, monitoring, secret management and multi-tenant E2E tests.
- No AI actions affecting billing, deletion, or bulk messaging without human approval.
- No secrets, customer PII or third-party tokens in commits, prompts, logs or fixtures.
