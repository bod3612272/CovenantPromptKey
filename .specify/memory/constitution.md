<!--
  SYNC IMPACT REPORT
  ==================
  Version Change: 1.0.0 → 2.0.0

  Modified Principles:
  - I. Code Quality Excellence → VI. Code Quality Excellence (renumbered, content preserved)
  - II. Testing-First Discipline → VII. Testing Discipline (renumbered, relaxed for brownfield)
  - III. User Experience Consistency → VIII. User Experience Consistency (renumbered)
  - IV. Performance Requirements → IX. Performance Requirements (renumbered)
  - V. Documentation Synchronization → X. Documentation Synchronisation (renumbered)

  Added Sections:
  - I. Brownfield Project Respect (NON-NEGOTIABLE) - New foundational principle
    - Absolute prohibition on unrequested refactoring
    - Preservation of existing architecture, patterns, and code style
    - Specific constraints list (5 prohibited actions)
  - II. Minimal Change Principle - New core principle
    - Each modification MUST be the smallest change to achieve the goal
    - Preservation of existing comments, formatting, and structure
    - Implementation standards (5 requirements)
  - III. Explicit Approval Principle - New core principle
    - Code changes require explicit user approval
    - Categorised list of changes requiring approval (6 categories)
    - Formal approval workflow (4 steps)
  - IV. Technology Stack Stability (NON-NEGOTIABLE) - New core principle
    - Framework version locking requirements
    - Absolute prohibitions (6 items)
    - Version upgrade risk documentation (5 risk categories)
  - V. Change Classification - New governance section
    - Permitted changes table (6 categories)
    - Prohibited changes table (6 categories)
  - Development Workflow section (Pre-Change, During, Post-Change)
  - Emergency Override Mechanism

  Removed Sections:
  - Performance Benchmarks (merged into Principle IX)
  - Detailed TDD workflow (relaxed for brownfield maintenance)

  Templates Requiring Updates:
  ✅ .specify/templates/plan-template.md - Constitution Check updated for brownfield principles
  ✅ .specify/templates/spec-template.md - Documentation Synchronisation preserved
  ✅ .specify/templates/tasks-template.md - Documentation sync checklist preserved
  ⚠️ .github/PULL_REQUEST_TEMPLATE.md - Requires brownfield compliance checklist update

  Follow-up TODOs:
  - Update PR template with brownfield compliance checklist

  Rationale: This Project Code has transitioned from a greenfield project to a production
  brownfield system. This major version change introduces protective principles
  that prioritise system stability, prevent unintended modifications, and require
  explicit approval for changes. The constitution now reflects the reality that
  unnecessary changes in a production system introduce risk without commensurate
  benefit. Quality principles are preserved but contextualised for maintenance work.
-->

# Constitution

**Project Type**: Brownfield Project (Production System)  
**Governance Priority**: This constitution supersedes all other development practices, guidelines, and preferences.

---

## Core Principles

### I. Brownfield Project Respect (NON-NEGOTIABLE)

**ABSOLUTELY PROHIBITED** to refactor without explicit request. **MUST** modify only the specifically requested portions. **MUST** preserve existing architecture, patterns, and code style.

**Rationale**: This is a production brownfield project. Unnecessary changes jeopardise system stability and team workflow safety.

**Specific Constraints**:

- ❌ PROHIBITED: Reorganising folder structure
- ❌ PROHIBITED: Changing naming conventions or coding style
- ❌ PROHIBITED: Introducing new design patterns (unless explicitly requested)
- ❌ PROHIBITED: Modifying the responsibility scope of existing classes
- ✅ PERMITTED: Modifying only code directly related to the requirement

---

### II. Minimal Change Principle

Each modification **MUST** be the smallest change to achieve the goal. **MUST** avoid touching unrelated files or code. **MUST** preserve existing comments, formatting, and structure.

**Rationale**: Minimising change scope reduces risk, simplifies review, and maintains system stability in the brownfield environment.

**Implementation Standards**:

- Limit change scope to a single functional module
- Preserve all existing comments (unless outdated or incorrect)
- Maintain existing indentation, line breaks, and whitespace formatting
- Avoid "whilst I'm here" optimisations or code cleanup
- New code MUST conform to the surrounding style

---

### III. Explicit Approval Principle

Code changes, architectural decisions, and package updates **REQUIRE** explicit user approval. **MUST** provide options and await confirmation. **ABSOLUTELY PROHIBITED** to unilaterally modify structure, add libraries, or change build configuration.

**Rationale**: Users retain control over all changes to protect production environment stability.

**Changes Requiring Approval**:

| Category | Examples |
|----------|----------|
| **Package Management** | Adding or removing NuGet packages |
| **Project Configuration** | Modifying `.csproj` or `.sln` files |
| **Dependency Injection** | Changing DI configuration |
| **Data Layer** | Modifying database schema or migration scripts |
| **API Contracts** | Changing API contracts or public interfaces |
| **Build/Deploy** | Adjusting build or deployment processes |

**Approval Workflow**:

1. Propose the change, explaining rationale and impact scope
2. Provide at least two alternative approaches (where applicable)
3. Await explicit user response: "Approved" or "Rejected"
4. Document the decision rationale in relevant documentation

---

### IV. Technology Stack Stability (NON-NEGOTIABLE)

**MUST** maintain the project's existing Framework version and technology stack. **ABSOLUTELY PROHIBITED** to upgrade or downgrade major versions without explicit approval. **MUST** follow existing SOLID principles and design patterns.

**Rationale**: Framework version changes trigger cascading breaking changes throughout the codebase. Technology stack stability in brownfield projects is the cornerstone of system reliability.

**Core Requirements**:

| Item | Requirement | Notes |
|------|-------------|-------|
| **Framework Lock** | MUST maintain current version | e.g., .NET 8.0 |
| **Language Version Lock** | MUST correspond to Framework | e.g., C# 12.0 |
| **Package Version Control** | Security patches only | Major versions MUST remain stable |
| **Platform Tooling** | MUST be Framework-compatible | Visual Studio version alignment |

**Absolute Prohibitions**:

- ❌ Upgrading Framework version without approval (e.g., .NET 6 → .NET 8)
- ❌ Downgrading Framework version without approval
- ❌ Modifying `<TargetFramework>` in `.csproj`
- ❌ Changing `<LangVersion>` settings
- ❌ Introducing packages incompatible with target Framework
- ❌ Modifying `<Nullable>` settings (if NRT already enabled)

**Version Upgrade Risks**:

1. **API Breaking Changes**: New versions may remove or modify existing APIs
2. **Dependency Conflicts**: Package version incompatibility issues
3. **Runtime Behaviour Differences**: Same code may behave differently across versions
4. **Deployment Environment Incompatibility**: Production may not support new versions
5. **Team Learning Cost**: Time required to adapt to new features and changes

---

### V. Change Classification

#### Permitted Changes (Require User Request)

| Category | Description | Example |
|----------|-------------|---------|
| **Bug Fixes** | Fixing specific programme errors | Fixing NullReferenceException |
| **Feature Addition** | Implementing new functionality in new files | Adding user authentication module |
| **Security Patches** | Updating packages with security vulnerabilities | Upgrading Newtonsoft.Json for CVE patch |
| **Documentation Updates** | Updating README, comments, API documentation | Adding API usage examples |
| **Test Addition** | Adding tests for existing or new functionality | Adding unit tests to improve coverage |
| **Performance Optimisation** | Targeted optimisation for specific bottlenecks | Optimising database query performance |

#### Prohibited Changes (Unless Explicitly Requested and Approved)

| Category | Description | Why Prohibited |
|----------|-------------|----------------|
| **Code Refactoring** | Reorganising existing code | High risk; may introduce new errors |
| **Architecture Changes** | Modifying project structure or layering | Impact scope too broad; difficult to verify |
| **Pattern Changes** | Modifying design patterns or conventions | Breaks code consistency |
| **Major Version Upgrades** | Upgrading Framework or major packages | Cascading breaking changes |
| **Feature Removal** | Deleting or reorganising existing functionality | May affect modules depending on this feature |
| **DI Configuration Changes** | Modifying dependency injection settings | Affects entire application object lifecycle |

---

### VI. Code Quality Excellence

**MUST** adhere to SOLID principles and .NET best practices:

- **Single Responsibility**: Each class/method has one well-defined purpose
- **Dependency Injection**: Use Microsoft.Extensions.DependencyInjection for IoC
- **Nullable Reference Types**: Enabled by default; all nullability explicitly annotated
- **Async/Await**: All I/O operations MUST use async/await with proper `ConfigureAwait` usage
- **Resource Management**: Implement `IDisposable` for unmanaged resources; use `using` statements
- **Exception Handling**: Structured exception handling with specific catch blocks; avoid catching `Exception` unless necessary
- **Code Documentation**: Public APIs MUST have XML documentation comments

**Rationale**: High-quality code reduces maintenance burden, improves reliability, and ensures the codebase remains comprehensible and extensible as the project scales. SOLID principles and .NET idioms prevent technical debt accumulation.

---

### VII. Testing Discipline

**MUST** ensure test coverage for new and modified functionality:

1. Existing tests **MUST** continue to pass after changes
2. New features **SHOULD** include tests
3. **ABSOLUTELY PROHIBITED** to modify or remove existing tests unless explicitly instructed

**Rationale**: The test suite protects the system from functional regression. Unified standards ensure test code consistency and maintainability.

#### Testing Framework Standards

| Category | MUST Use | MUST NOT Use |
|----------|----------|--------------|
| Test Framework | **NUnit 4.x** | xUnit, MSTest |
| Mocking Library | **NSubstitute 5.x** | Moq, FakeItEasy |
| Assertion Syntax | **FluentAssertions 6.x** | Assert.* native syntax |

#### Test Pattern and Naming

**AAA Pattern** (MUST follow):

```csharp
[Test]
public void MethodName__TestScenario__ExpectedResult()
{
    // 準備 (Arrange)
    var sut = new SystemUnderTest();
    
    // 執行 (Act)
    var result = sut.MethodUnderTest();
    
    // 驗證 (Assert)
    result.Should().Be(expectedValue);
}
```

**Naming Convention**:

- Test classes: `{ClassName}Tests`
- Test methods: `MethodName__TestScenario__ExpectedResult()`
- Use Traditional Chinese (Taiwan) to describe scenarios and results

#### Coverage Requirements

| Category | Coverage | Notes |
|----------|----------|-------|
| New functionality | ≥ 80% | Baseline requirement for new code |
| Core business logic | ≥ 90% | Critical logic MUST achieve this |
| Existing functionality modifications | MUST NOT decrease | Preserve existing coverage |

#### NUnit Attribute Markers

**MUST use**:

- `[TestFixture]`: Mark test classes
- `[Test]`: Mark test methods
- `[TestCase]`: Parameterised tests
- `[SetUp]`: Test setup operations
- `[TearDown]`: Test cleanup operations

---

### VIII. User Experience Consistency

**MUST** maintain consistent, predictable user interactions:

- **Error Messages**: Clear, actionable messages with error codes (e.g., "ERR001: Invalid log file path. Ensure the file exists and you have read permissions.")
- **Output Formatting**: Consistent formatting across all commands (support both JSON and human-readable text formats)
- **Progress Feedback**: Long-running operations MUST provide progress indicators or feedback
- **Input Validation**: Validate all user inputs with helpful error messages before processing
- **Exit Codes**: Standard exit codes (0 for success, non-zero for failures with documented meanings)
- **Help Documentation**: Every command/feature MUST have accessible help text
- **Accessibility**: Consider terminal compatibility and screen reader support

**Rationale**: Consistent UX reduces cognitive load, minimises user errors, and creates trust. Users should never be surprised by inconsistent behaviour or unclear feedback. Good UX is a quality attribute, not an afterthought.

---

### IX. Performance Requirements

**MUST** meet quantifiable performance standards:

- **Startup Time**: Application cold start < 2 seconds
- **Response Time**: Interactive operations < 200ms for p95 (95th percentile)
- **Memory Efficiency**: Base memory footprint < 50MB; < 200MB under load
- **Throughput**: Log processing ≥ 10,000 lines/second for typical log files
- **Scalability**: Handle log files up to 1GB without degradation
- **Resource Cleanup**: No memory leaks; proper disposal of all resources

**Performance Validation**:

- Benchmark critical paths using BenchmarkDotNet
- Profile memory usage with dotMemory or PerfView
- Include performance regression tests in CI pipeline
- Document performance characteristics in code comments
- Store baseline results in repository (`benchmarks/baselines/`)
- CI pipeline MUST flag performance regressions > 10%

**Optimisation Guidelines**:

- Use `Span<T>`, `Memory<T>`, and `ArrayPool<T>` for high-throughput scenarios
- Minimise allocations in hot paths
- Prefer `ValueTask<T>` over `Task<T>` for frequently-called async methods
- Stream large files rather than loading entirely into memory

**Rationale**: Performance is a feature, not an optimisation. Users expect fast, responsive tools. Quantifiable metrics prevent performance regressions and ensure the application remains usable as scale increases.

---

### X. Documentation Synchronisation (NON-NEGOTIABLE)

**MUST** maintain documentation consistency across all user-facing artefacts:

- **Feature Completion Requirement**: Upon completing ANY spec implementation (new feature, modification, or bug fix that affects user behaviour), the `docs/getting-started.md` MUST be updated BEFORE the task is considered complete.
- **Update Scope**: Documentation updates MUST include:
  - New API usage examples with complete code snippets
  - Updated parameter tables if `SearchOptions` or other public APIs change
  - Revised troubleshooting sections if error behaviours change
  - Performance benchmark updates if measurable characteristics change
  - Version history entries reflecting the change
- **Verification Checklist**: Before closing any spec/task, confirm:
  - ✅ All new public APIs documented with examples
  - ✅ Breaking changes clearly marked and migration paths provided
  - ✅ Real-world usage scenarios added/updated where applicable
  - ✅ Table of Contents updated to reflect new sections
  - ✅ Cross-references to related documentation validated
- **Documentation Review**: All documentation changes MUST undergo the same code review rigour as implementation code—inaccurate or outdated documentation is a critical defect.
- **Synchronisation Points**:
  - `docs/getting-started.md` – Primary user-facing NuGet package guide
  - `README.md` – Project overview and quick links
  - `CHANGELOG.md` – Version history and migration notes
  - Inline XML documentation comments on public APIs
  - `specs/*/spec.md` – Technical specifications for features

**Rationale**: Outdated documentation destroys user trust and wastes engineering time debugging perceived bugs that are actually documentation errors. Documentation is not a post-release activity—it is an integral part of feature delivery. Users judge library quality by documentation completeness and accuracy. Synchronous updates prevent documentation drift and ensure every release includes complete, trustworthy guidance.

---

## Development Workflow

### Pre-Change

1. **Read the Constitution**: Confirm the change complies with all principles
2. **Identify Scope**: Clearly define files and modules affected by the change
3. **Propose the Change**: Explain to the user the change content, rationale, and impact
4. **Await Approval**: Obtain explicit "Approved" response before commencing work

### During Implementation

1. **Minimise Modifications**: Modify only necessary code
2. **Preserve Style**: Maintain existing coding style and formatting
3. **Follow Standards**: Use the project's existing Framework and language features
4. **Write Tests**: Add tests for new functionality (AAA pattern)
5. **Verify Side Effects**: Confirm the change does not affect other modules

### Post-Change

1. **Report Changes**: List all modified files and change contents
2. **Confirm Tests**: Execute all tests and report results
3. **Verify Warnings**: Check compilation warnings (especially NRT warnings)
4. **Await Verification**: Wait for user verification that changes meet expectations

---

## Development Standards

**Code Style**:

- Follow Microsoft C# Coding Conventions
- Use EditorConfig for consistent formatting
- Enable all nullable reference type warnings
- Treat warnings as errors in Release builds

**Dependency Management**:

- Minimise external dependencies
- Justify each NuGet package addition
- Keep dependencies updated (security patches within 30 days)
- Document dependency choices in architecture decision records (ADRs)

**Observability**:

- Structured logging using Microsoft.Extensions.Logging or Serilog
- Log levels used appropriately (Trace/Debug/Information/Warning/Error/Critical)
- Include correlation IDs for request tracing
- Performance counters for critical operations

---

## Governance

### Priority

This constitution **supersedes** all other development guidelines, coding standards, and best practice recommendations. Any AI agent, developer, or tool **MUST** unconditionally comply with these principles.

### Amendment Process

1. **Amendment Trigger**: Constitution changes require explicit user request
2. **Rationale Recording**: Amendment rationale and impact scope MUST be documented
3. **Version Control**: Follow Semantic Versioning
   - MAJOR: Incompatible principle changes
   - MINOR: Backward-compatible principle additions
   - PATCH: Error corrections or clarifications
4. **Synchronous Updates**: Update all related templates and documents

### Compliance Verification

Every code change **MUST** undergo compliance checking before implementation:

**Checklist**:

- [ ] Has the change received explicit user approval?
- [ ] Is the change scope minimised?
- [ ] Has the existing code style been preserved?
- [ ] Has unrequested refactoring been avoided?
- [ ] Has Framework version stability been maintained?
- [ ] Does new functionality include tests?
- [ ] Do existing tests still pass?

**Handling Uncertainty**: If there is any doubt, **MUST** ask the user rather than assume or decide unilaterally.

### Emergency Override Mechanism

Users may temporarily suspend constitution principles via the following commands:

- **Global Suspension**: "ignore constitution" or "suspend constitution"
- **Specific Principle Suspension**: "suspend Principle III: Explicit Approval Principle"
- **Restoration**: "restore constitution" or "resume constitution"

**Note**: Emergency overrides apply only to the current conversation and do not affect the constitution document itself.

---

**Version**: 2.0.0 | **Ratified**: 2025-12-04 | **Last Amended**: 2025-12-04
