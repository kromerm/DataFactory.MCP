# Bug Bash — Fabric Apache Airflow Job via DataFactory.MCP

**Audience:** Data Factory PMs
**Window:** This week (May 18–22, 2026)
**Goal:** Exercise the new Apache Airflow Job tooling in `DataFactory.MCP` by driving it through GitHub Copilot. Find bugs, friction, and gaps in the end-to-end PM/developer experience — from "create a job with a DAG" to "inspect the project definition in my workspace."

> Repo under test: <https://github.com/microsoft/DataFactory.MCP>
> Feature spec: `docs/airflow-job-feature.md`
> Tests this bash is modeled on: `DataFactory.MCP.Tests/Integration/AirflowJobToolIntegrationTests.cs` (notably `AirflowJob_FullLifecycle_CreateGetUpdateDeleteAsync` and `GetAirflowJobDefinitionAsync_*`).

---

## Why we're doing this

Apache Airflow Job is the newest first-class artifact in Fabric Data Factory and the most recent addition to the DataFactory.MCP server. PMs are the closest proxy to "a Fabric customer using Copilot to manage Airflow." This bash validates that the **MCP tool surface** plus **Copilot reasoning** equals a real, usable authoring loop — not just a passing test suite.

We're specifically looking for:

- Tool descriptions that confuse Copilot (wrong tool picked, wrong parameters inferred)
- Error messages that don't tell the user what to do next
- Definition payload friction (base64, JSON shape, DAG file naming)
- Workspace/capacity prerequisites that aren't surfaced until something fails
- Missing tools or missing fields you'd reach for as a PM

---

## Prerequisites

1. **Clone & build** `DataFactory.MCP` locally (`.NET 10 SDK`):
   ```
   git clone https://github.com/microsoft/DataFactory.MCP
   cd DataFactory.MCP
   dotnet build
   ```
2. **VS Code + GitHub Copilot Chat** with the MCP server configured (`.vscode/mcp.json` ships with the repo). Restart the MCP server after pulling latest.
3. **Fabric workspace with a capacity** (`capacityId` must be non-null — free/PPU workspaces will fail). Use `mcp_datafactorymc_list_workspaces` to find one.
4. **Authenticate once per session** via Copilot:
   ```
   Use mcp_datafactorymc_authenticate_interactive to sign me in.
   ```

> If you hit `Skipping authenticated test - no valid credentials available` in the test suite, your token isn't cached. Re-authenticate.

---

## How to file a bug

- **Repo:** DataFactory.MCP → Issues → "New issue" → label `bug-bash` + `airflow`
- **Title format:** `[BugBash][Airflow] <short symptom>`
- **Include:** Copilot prompt you used, tool(s) Copilot invoked, full tool response, expected vs. actual, workspace ID (scrub if sensitive), and OS/VS Code/MCP server version.
- **Severity guide:**
  - **Sev 1** — data loss, auth broken, can't create or delete a job
  - **Sev 2** — wrong tool selected, definition round-trip corrupts content, misleading error
  - **Sev 3** — UX/wording, missing field in response, slow but works
  - **Sev 4** — nit, polish

---

## Test Case 1 — Create a new Airflow project with a DAG (end-to-end authoring)

**Modeled on:** `AirflowJob_FullLifecycle_CreateGetUpdateDeleteAsync`
**Tools exercised:** `create_airflow_job`, `get_airflow_job`, `update_airflow_job_definition`, `delete_airflow_job`
**Estimated time:** 10–15 minutes

### What the PM does (in Copilot Chat)

Drive the entire flow through natural language. Don't hand Copilot the tool names — see if it picks the right ones.

1. **Pick a workspace.**
   > "List my Fabric workspaces and pick one that has a capacity assigned."
   - **Expected:** Copilot calls `list_workspaces` and reports back at least one workspace with a non-null `capacityId`.

2. **Create the Airflow project.**
   > "Create a new Apache Airflow Job in workspace `<id>` called `bugbash-dag-<your-alias>` with the description `Bug bash test — taxi DAG`."
   - **Expected:** Copilot calls `create_airflow_job`. Response is valid JSON containing `airflowJobId`. Job appears when you list.

3. **Upload a DAG definition.** Paste this minimal scheduler config and ask Copilot to upload it as the job's definition:
   ```json
   {
     "schedulerConfig": {
       "dags": [
         { "fileName": "dag_bronze_ingest_yellow_taxi.py", "schedule": "0 * * * *" }
       ]
     }
   }
   ```
   > "Use this as the definition for the job we just created."
   - **Expected:** Copilot calls `update_airflow_job_definition` with `definitionJson` set to the JSON above (no manual base64 — the tool encodes). Returns success.

4. **Round-trip verify.**
   > "Now fetch the definition back and show me the decoded JSON."
   - **Expected:** Copilot calls `get_airflow_job_definition`. Returned JSON matches what you uploaded (no base64 leakage, no escaped quotes, no truncation).

5. **Clean up.**
   > "Delete that job permanently."
   - **Expected:** Copilot calls `delete_airflow_job` with `hardDelete: true`. Job no longer appears in list.

### What to watch for

- [ ] Copilot picks `create_airflow_job` (not `create_data_pipeline` or similar) on first try.
- [ ] `displayName` validation matches Fabric's actual rules (length, special chars).
- [ ] `update_airflow_job_definition` accepts the JSON without the user having to base64-encode by hand.
- [ ] The round-tripped JSON in step 4 is **byte-equal** (or at least structurally equal) to step 3's input.
- [ ] Errors when the workspace has no capacity are clear and actionable (not a raw 4xx).
- [ ] Delete actually removes the artifact — verify in the Fabric portal too.

### Pass / Fail
- **Pass:** All 5 steps succeed via natural-language prompts; round-trip preserves content; cleanup is clean.
- **Fail:** Any step requires manual tool-name correction, raw JSON editing the user shouldn't have to do, or leaves orphaned artifacts.

---

## Test Case 2 — Query a Fabric workspace for an Airflow project definition

**Modeled on:** `GetAirflowJobDefinitionAsync_*` + `ListAirflowJobsAsync_WithAuthentication`
**Tools exercised:** `list_airflow_jobs`, `get_airflow_job`, `get_airflow_job_definition`
**Estimated time:** 5–10 minutes

### Setup
Use an Airflow Job that already exists in your workspace (either one you created in Test Case 1 before deleting, or any pre-existing job). If you have none, run Test Case 1 steps 1–3 first and skip the delete.

### What the PM does (in Copilot Chat)

1. **Discover jobs in the workspace.**
   > "What Apache Airflow Jobs do I have in workspace `<id>`?"
   - **Expected:** Copilot calls `list_airflow_jobs`. Returns a list with `id`, `displayName`, `description` for each. Pagination via continuation token works if there are many.

2. **Inspect metadata for a specific job.**
   > "Tell me about the job named `bugbash-dag-<your-alias>` — when was it created and who owns it?"
   - **Expected:** Copilot resolves name → `airflowJobId` (either by filtering the list or asking you), calls `get_airflow_job`, and surfaces the displayName, description, and any timestamps/owner fields the API returns.

3. **Fetch the definition.**
   > "Show me the full DAG definition for that job."
   - **Expected:** Copilot calls `get_airflow_job_definition`. Response is the decoded definition JSON (not base64).

4. **Ask a comprehension question about the definition.**
   > "Which DAG files are in that project and what are their schedules?"
   - **Expected:** Copilot parses the returned JSON and answers in natural language — e.g., "`dag_bronze_ingest_yellow_taxi.py` runs hourly (`0 * * * *`)." This tests whether the tool response shape is *Copilot-readable*, not just machine-parseable.

5. **Negative case — non-existent job.**
   > "Get the definition for Airflow job `00000000-0000-0000-0000-000000000001` in this workspace."
   - **Expected:** Copilot calls `get_airflow_job_definition` and reports a clear "not found" style error — not a raw stack trace, not an auth error.

### What to watch for

- [ ] `list_airflow_jobs` returns enough fields that the PM can pick a job without a second call.
- [ ] Name → ID resolution in step 2 is graceful (Copilot doesn't get stuck).
- [ ] The definition in step 3 is **decoded** before Copilot shows it — no `eyJzY2hlZHVsZXJDb25maWciOnt9fQ==` strings.
- [ ] Step 4 succeeds — i.e., the response shape lets the LLM answer DAG-level questions. If Copilot has to "guess," the response shape needs work.
- [ ] Step 5 returns a *useful* error (mentions the job ID, suggests listing jobs).
- [ ] No tool returns an empty string or a 500 in any of these cases.

### Pass / Fail
- **Pass:** All 5 steps succeed; Copilot answers the DAG comprehension question correctly from a single `get_airflow_job_definition` response.
- **Fail:** Any step requires raw base64 decoding by the user, multiple redundant tool calls, or returns a confusing error.

---

## Scorecard (fill this in and post in the bug bash channel when done)

| Field | Your entry |
|---|---|
| PM name | |
| Workspace ID used | |
| MCP server version | (run `git rev-parse --short HEAD`) |
| TC1 result | Pass / Fail |
| TC2 result | Pass / Fail |
| # bugs filed | |
| Top friction point | (one sentence) |
| One thing we should ship next | (one sentence) |

---

## Stretch goals (if you finish early)

- Try authoring a DAG that references a Fabric Lakehouse path — does the definition tool care about the contents or is it pass-through?
- Try `update_airflow_job` to rename a job mid-flight; verify the new name appears in `list_airflow_jobs` without a refresh delay.
- Soft-delete vs. hard-delete: does a soft-deleted job appear in `list_airflow_jobs`? Should it?
- Cross-workspace: can Copilot copy a definition from one workspace to another using two tool calls? (No native "copy" tool exists — see if Copilot improvises sensibly.)

---

*Bug bash owner: @makromer · Questions: post in the Data Factory MCP Teams channel.*
