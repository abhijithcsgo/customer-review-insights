# Security Notes

## Findings from repository inspection

| Item | Status | Guidance |
| --- | --- | --- |
| Gemini key in source / `appsettings.json` | Empty placeholder only | Keep using User Secrets / host env |
| Frontend Gemini exposure | Not present | Key must remain backend-only |
| `client/.env` on disk | Local file; ignored by `.gitignore` | Do not commit; contains no Gemini key by design (API URL only) |
| Connection string in `appsettings.json` | Local Windows auth template (`Trusted_Connection`) | Replace for production; do not commit SQL passwords |
| Git tracked secrets | None in the tracked tree. `appsettings.json` has an empty Gemini key and a local Windows-auth connection string (no password). | `client/.env` stays untracked and only sets `VITE_API_BASE_URL` |
| CORS | Localhost origins only | Narrow to real origins in production |

## Handling accidental secret exposure

1. **Do not** paste the secret into chat, tickets, or docs.
2. Revoke/rotate the Gemini key in Google AI Studio.
3. Set the new key via User Secrets or host configuration.
4. If a secret was committed historically, remove it from the working tree (`git rm --cached <file>`), commit, then rotate the secret. History rewrite is optional and should be coordinated with the team.

`.gitignore` prevents *new* adds; it does not remove already-tracked files.
