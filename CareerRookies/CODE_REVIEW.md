# FULL CODE REVIEW - CareerRookies

**Date:** 2026-03-10
**Reviewer:** Claude (AI-assisted)
**Project:** CareerRookies.Web (.NET 8.0 ASP.NET Core MVC)

---

## PART 1: BUGS & ISSUES THAT NEED FIXING

### CRITICAL

| # | Issue | File(s) | Details |
|---|-------|---------|---------|
| 1 | **jQuery never loaded** | `_Layout.cshtml`, `_AdminLayout.cshtml` | jQuery Validate is included in `_ValidationScriptsPartial.cshtml` but jQuery itself is never loaded. **All client-side form validation is completely broken.** |
| 2 | **Stored XSS on article content** | `Article/Detail.cshtml:30` | `@Html.Raw(Model.Content)` renders user-submitted article content as raw HTML. Users submit articles publicly, admin approves but never edits the content. A `<script>` tag passes through approval. |
| 3 | **Plaintext credentials in source control** | `appsettings.json`, `appsettings.Production.json` | DB password (`demo-pass2-work`) and admin password (`Admin123!`) are committed in plaintext. |
| 4 | **File upload has no validation** | `WorkshopManagementController.SaveFileAsync` | No file type whitelist, no size limit, no MIME validation. An admin could upload `.exe`, `.aspx`, `.cshtml` files. Extension comes from user-supplied filename. |

### HIGH

| # | Issue | File(s) | Details |
|---|-------|---------|---------|
| 5 | **Admin panel broken on mobile** | `_AdminLayout.cshtml`, `site.js` | No sidebar toggle button exists in HTML. `site.js` looks for `[data-toggle-sidebar]` which doesn't exist. Sidebar is hidden on `<992px` with no way to open it. |
| 6 | **DateTime.Now vs DateTime.UtcNow everywhere** | All controllers + models | Models default to `DateTime.UtcNow`, controllers use `DateTime.Now`. Workshop date comparisons are inconsistent. Workshops may show as upcoming/past incorrectly depending on server timezone. |
| 7 | **Over-posting / mass assignment** | `ResourceManagement`, `StudentClassManagement`, `TestimonialManagement`, `WorkshopManagement` controllers | All bind directly to domain models. Attacker can set `Id`, `CreatedAt`, navigation properties via form manipulation. Should use ViewModels. |
| 8 | **GDPR consent not enforced at model level** | `WorkshopRegistration.cs:25` | `[Required]` on `bool` is a no-op (bool is never null). Only the ViewModel has `[Range(typeof(bool), "true", "true")]`. Direct DB inserts can have `consent = false`. |
| 9 | **Stored XSS on workshop descriptions** | `Workshop/Detail.cshtml:53,112` | `@Html.Raw()` on `Description` and `SpeakerDescription`. Admin-only input but still unsafe in multi-admin scenarios. |
| 10 | **No duplicate registration check** | `WorkshopController.Register` | A student can register for the same workshop unlimited times. No uniqueness check. |
| 11 | **Can register for past workshops** | `WorkshopController.Register` | No guard checking `if (workshop.Date <= DateTime.Now)`. Registration form still works for past events. |
| 12 | **Malformed connection string** | `appsettings.json:3` | `Server=.\\MSSQLSERVER2022:0` -- the `:0` after a named instance is invalid syntax. Should be `Server=.\\MSSQLSERVER2022`. |

### MEDIUM

| # | Issue | File(s) | Details |
|---|-------|---------|---------|
| 13 | **YouTube toggle JS broken** | `Admin/Workshop/Media.cshtml:153` | Compares `select.value` (enum name like `"YouTubeLink"`) against `@((int)MediaType.YouTubeLink)` (integer `2`). They never match. YouTube URL field never shows. |
| 14 | **Sidebar width mismatch** | `_AdminLayout.cshtml` vs `site.css` | Inline style sets sidebar to 250px, `site.css` sets it to 260px. Creates a 10px visual gap. |
| 15 | **Duplicate TempData alerts** | `Workshop/Detail.cshtml`, `Article/Index.cshtml` | These views render TempData alerts, but `_Layout.cshtml` already renders them globally. Success messages appear twice. |
| 16 | **UpdatedAt never auto-updates** | `Workshop.cs`, `Article.cs` | `DateTime.UtcNow` is set once in the initializer. No `SaveChanges` override or EF interceptor updates it on subsequent edits. Controllers must do it manually (some forget). |
| 17 | **Synchronous DB calls at startup** | `Program.cs:38`, `SeedData.cs` | `Database.Migrate()` (not async) blocks the startup thread. `SeedData` uses `Any()` instead of `AnyAsync()`. |
| 18 | **SettingsController N+1 query** | `SettingsController.Update` | Each setting key triggers a separate `FirstOrDefaultAsync` query in a loop. Should load all settings at once. |
| 19 | **Orphaned files on disk** | `WorkshopManagementController` | Deleting workshops/media doesn't delete physical files. Updating images doesn't delete old files. Files accumulate. |
| 20 | **StudentClass entity is disconnected** | `StudentClass.cs`, `WorkshopRegistration.cs` | `WorkshopRegistration.StudentClass` is a free-text string, not an FK to the `StudentClass` table. The table exists but isn't enforced. |
| 21 | **Wildcard NuGet versions** | `CareerRookies.Web.csproj` | `Version="8.0.*"` for EF Core packages. Builds are non-reproducible and can break unexpectedly. |
| 22 | **Seed error silently swallowed** | `SeedData.cs:48` | `CreateAsync` failures are never logged. If user creation fails, no error is reported. |
| 23 | **No null check in SettingsController** | `SettingsController.Update:32` | If `settings` dictionary parameter is null, `foreach` throws `NullReferenceException`. |

### LOW

| # | Issue | File(s) | Details |
|---|-------|---------|---------|
| 24 | **Home/Privacy.cshtml is placeholder** | `Views/Home/Privacy.cshtml` | Default ASP.NET template English text. Real privacy page is at `/Legal/Privacy`. Confusing duplicate route. |
| 25 | **_Layout.cshtml.css has conflicting styles** | `_Layout.cshtml.css` | Default template blue colors conflict with CareerRookies brand. Dead CSS file. |
| 26 | **Social media links are `#`** | `_Layout.cshtml`, `Home/Index.cshtml` | All social media footer links are placeholders. |
| 27 | **Placeholder phone number** | `Home/Index.cshtml:220` | `+40 700 000 000` is clearly fake. |
| 28 | **Duplicate Google Fonts loading** | `site.css` + both layouts | `@import` in CSS AND `<link>` in HTML. Double bandwidth. |
| 29 | **No pagination anywhere** | All Index actions | Every list page loads entire tables into memory. |
| 30 | **Missing Romanian diacritics** | Admin views | "in" instead of "in", etc. throughout admin interface. |
| 31 | **Testimonial role shown twice** | `Testimonial/Index.cshtml` | Author type displayed both in role div and badge. |
| 32 | **Article reject vs pending indistinguishable** | `ArticleManagementController` | "Reject" just sets `IsApproved = false`, same as default. No way to tell pending from rejected. |
| 33 | **DashboardController uses ViewBag** | `DashboardController.cs` | 6 dynamic ViewBag properties instead of a ViewModel. Typo = silent null. |
| 34 | **No `[Url]` validation on CareerResource.Url** | `CareerResource.cs` | `[DataType(DataType.Url)]` is display-only, doesn't validate format. |
| 35 | **HomeViewModel missing properties** | `HomeViewModel.cs` | No `Articles` or `CareerResources` properties for homepage sections the seed data implies. |

---

## PART 2: IMPROVEMENTS — STATUS

### Architecture & Code Quality

1. [x] **Add a service/repository layer** -- 9 service interfaces + implementations (Workshop, Article, Testimonial, Resource, Settings, StudentClass, File, Audit, Dashboard).
2. [x] **Add ViewModels for all admin Create/Edit actions** -- WorkshopFormViewModel, ResourceFormViewModel, TestimonialFormViewModel, StudentClassFormViewModel.
3. [x] **Add a `SaveChanges` override with `ITimestamped` interface** -- Generic implementation for all entities via `ITimestamped` interface.
4. [x] **Standardize on `DateTime.UtcNow` everywhere** -- All services and models use UtcNow consistently.
5. [x] **Add global exception handling** -- Fixed Error.cshtml NullReferenceException (nullable model), added Serilog for structured error logging.

### Security

6. [x] **Add rate limiting** -- `PublicForm` (5 req/min) and `General` (60 req/min) policies via `Microsoft.AspNetCore.RateLimiting`.
7. [x] **Add CAPTCHA to public forms** -- Google reCAPTCHA v2 integrated. `RecaptchaService` + `IRecaptchaService`. Auto-skips if keys not configured. Added to Article Submit and Workshop Registration forms. Configure `Recaptcha:SiteKey` and `Recaptcha:SecretKey` in appsettings or environment variables.
8. [x] **HTML sanitization library** -- `HtmlSanitizer` NuGet (v9.0.892). `HtmlSanitizerService` sanitizes article content on submit and workshop descriptions on save. Safe tags allowed (headings, lists, links, tables, images). `@Html.Raw()` now safe in Article/Detail and Workshop/Detail views.
9. [x] **Move secrets to environment variables** -- Production uses `web.config` env vars. `appsettings.json` has placeholder for AdminSettings:Password.
10. [x] **Add CORS policy** -- `AddCors` with configurable `Cors:AllowedOrigins` array. Production config includes `careerrookies.ro` and `www.careerrookies.ro`. `app.UseCors("Default")` in pipeline.
11. [x] **Add Content Security Policy headers** -- `SecurityHeadersMiddleware` adds CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy.

### Features

12. [x] **Pagination** -- `PagedResult<T>` with `_Pagination.cshtml` partial. All list pages (public + admin) now paginated.
13. [x] **Article rich text editor** -- TinyMCE 7 via CDN. Configured for Article Submit (public) and Workshop Create/Edit (admin) with safe toolbar (formatting, lists, links). Romanian language. Content sanitized server-side via HtmlSanitizer.
14. [x] **Testimonial moderation** -- `IsApproved` flag with admin approve/reject workflow. Only approved testimonials shown publicly.
15. [x] **Article status enum** -- `ArticleStatus { Pending, Approved, Rejected }` with full admin workflow.
16. [x] **Workshop capacity limit** -- `MaxCapacity` field, registration check in `CanRegisterAsync`, UI shows capacity in admin and public views.
17. [x] **Link StudentClass to WorkshopRegistration via FK** -- Proper FK relationship configured.
18. [ ] **Article author tracking via Identity** -- Requires authentication on article submit. Future enhancement.
19. [x] **SEO-friendly URLs** -- `Slug` property on Article + Workshop with Romanian diacritics handling. Routes: `/Article/{slug}`, `/Workshop/{slug}`.
20. [x] **Image optimization** -- `SixLabors.ImageSharp` (v3.1.12). Images auto-resized to max 1920x1080, saved as JPEG at 80% quality. Thumbnails generated (400x300) in `/thumbs/` subfolder. Fallback to original on failure. Old thumbnails cleaned up on delete.
21. [ ] **Email notifications** -- Requires SMTP configuration. Future enhancement.
22. [x] **Search functionality** -- `Home/Search` action with article search across title, content, author.
23. [ ] **Workshop categories/tags** -- Requires new entity + UI. Future enhancement.
24. [ ] **Analytics dashboard** -- Requires charting library. Future enhancement.
25. [x] **Export registrations** -- CSV export with BOM for Excel compatibility. `/Admin/Workshops/Registrations/{id}/Export`.
26. [ ] **Multi-language support** -- Major effort with `.resx` files. Future enhancement.
27. [x] **Soft delete** -- `ISoftDeletable` interface with `IsDeleted`/`DeletedAt`. Global query filters in EF Core. Applied to Workshop, Article, Testimonial.
28. [x] **Audit log** -- `AuditLog` entity with entity type, ID, action, user email, timestamp. All admin CRUD actions logged.

### Performance

29. [x] **Add response caching** -- `AddResponseCaching()` middleware registered.
30. [x] **Add output caching middleware** -- .NET 8 `AddOutputCache()` with 5-min default and 1-hour "Static" policy.
31. [x] **Optimize dashboard queries** -- `DashboardService` combines workshop counts into single GroupBy query.
32. [x] **Add database indexes** -- On `Article.Status`, `Article.Slug`, `Workshop.Date`, `Workshop.Slug`, `Testimonial.IsApproved`, `AuditLog.Timestamp`, composite index on `WorkshopRegistration(WorkshopId, StudentName, StudentClassId)`.

### DevOps

33. [x] **Pin NuGet package versions** -- All packages pinned to exact versions (8.0.23).
34. [ ] **Add unit tests** -- Requires separate test project. Future enhancement.
35. [ ] **Add CI/CD pipeline** -- Requires GitHub Actions / Azure DevOps configuration.
36. [x] **Add health check endpoint** -- `app.MapHealthChecks("/health")` with DB context check.
37. [x] **Add structured logging** -- Serilog with console + rolling file sinks (`logs/log-{Date}.txt`).
