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

## PART 2: IMPROVEMENTS FOR LATER

### Architecture & Code Quality

1. **Add a service/repository layer** -- All controllers use `DbContext` directly. Extract business logic into services for testability and reuse.
2. **Add ViewModels for all admin Create/Edit actions** -- Prevents over-posting and separates concerns.
3. **Add a `SaveChanges` override in `ApplicationDbContext`** -- Auto-set `UpdatedAt` on all entities implementing an `ITimestamped` interface.
4. **Standardize on `DateTime.UtcNow` everywhere** -- Or use `DateTimeOffset` for timezone-aware timestamps.
5. **Add global exception handling middleware** -- Return friendly error pages instead of 500s.

### Security

6. **Add rate limiting** -- Especially on `ArticleController.Submit` (public form). Use `Microsoft.AspNetCore.RateLimiting`.
7. **Add CAPTCHA to public forms** -- Article submission and workshop registration need anti-spam.
8. **HTML sanitization library** -- Use HtmlSanitizer NuGet for article content instead of `@Html.Raw()`. Strip dangerous tags, keep formatting.
9. **Move secrets to User Secrets / environment variables** -- Remove all passwords from `appsettings*.json`.
10. **Add CORS policy** -- If any API endpoints are added later.
11. **Add Content Security Policy headers** -- Prevent XSS execution.

### Features

12. **Pagination** -- Add to all list pages (articles, testimonials, resources, admin tables).
13. **Article rich text editor** -- Use TinyMCE or similar for article submission instead of plain textarea.
14. **Testimonial moderation** -- Add `IsApproved` flag to `Testimonial` model with admin approval workflow.
15. **Article status enum** -- Replace `bool IsApproved` with `enum ArticleStatus { Pending, Approved, Rejected }`.
16. **Workshop capacity limit** -- Add `MaxCapacity` to `Workshop` model, check before allowing registration.
17. **Link StudentClass to WorkshopRegistration via FK** -- Replace the free-text string with a proper foreign key.
18. **Article author tracking** -- Link articles to `IdentityUser` instead of just a free-text `AuthorName`.
19. **SEO-friendly URLs** -- Add `Slug` property to `Article` and `Workshop`.
20. **Image optimization** -- Resize/compress uploaded images. Add alt text fields for accessibility.
21. **Email notifications** -- Confirm workshop registration, notify admin of new article submissions.
22. **Search functionality** -- Add search across articles, workshops, resources.
23. **Workshop categories/tags** -- Allow filtering workshops by topic.
24. **Analytics dashboard** -- Show registration trends, popular articles, etc. in admin.
25. **Export registrations** -- Add CSV/Excel export on the admin Registrations page.
26. **Multi-language support** -- Use `.resx` resource files instead of hardcoded Romanian strings.
27. **Soft delete** -- Add `IsDeleted` flag instead of permanent deletion.
28. **Audit log** -- Track who changed what and when in the admin panel.

### Performance

29. **Add response caching** -- For public pages that don't change often (Legal, Resources).
30. **Add output caching middleware** -- `.NET 8` has built-in output caching.
31. **Optimize dashboard queries** -- Combine 6 `CountAsync` calls into a single query.
32. **Add database indexes** -- On frequently queried columns (`Article.IsApproved`, `Workshop.Date`, etc.).

### DevOps

33. **Pin NuGet package versions** -- Replace `8.0.*` wildcards with exact versions.
34. **Add unit tests** -- At minimum for controllers and any future service layer.
35. **Add CI/CD pipeline** -- GitHub Actions or Azure DevOps for build/test/deploy.
36. **Add health check endpoint** -- `app.MapHealthChecks("/health")` for monitoring.
37. **Add structured logging** -- Use Serilog with structured logging to file/seq.
