using MI.CRM.API.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;

namespace MI.CRM.API.Documents
{
    public class ReportDocument : IDocument
    {
        private readonly ReportDto _data;

        public ReportDocument(ReportDto data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            // -------- Page 1: Header / Title --------
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // Header / Logo Section
                    column.Item().Container().PaddingBottom(20).Column(inner =>
                    {
                        var logoPath = GetLogoPath();

                        if (File.Exists(logoPath))
                        {
                            byte[] logoBytes = File.ReadAllBytes(logoPath);
                            inner.Item().AlignLeft().Container().Width(120).PaddingBottom(20).Image(logoBytes).FitArea();

                        }
                        else
                        {
                            inner.Item().AlignLeft().Text("LOGO").SemiBold().FontColor(Colors.Grey.Darken1);
                        }

                        inner.Item().Text(_data.Project.AwardNumber ?? "-")
                            .FontSize(14)
                            .LetterSpacing(0.5f)
                            .FontColor(Colors.Black);

                        inner.Item().Text(_data.Project.Title ?? "-")
                            .FontSize(22)
                            .Bold()
                            .LineHeight(1.3f)
                            .FontColor(Colors.Black);
                    });
                });
            });

            // -------- Page 2: Project Overview --------
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                const float cellSpacing = 6f; // same for both horizontal & vertical

                page.Content().Column(column =>
                {
                    column.Spacing(cellSpacing);

                    // Title
                    column.Item().AlignCenter().Text("Project Overview")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    // Header Row (Name | Value)
                    column.Item().Row(row =>
                    {
                        row.Spacing(cellSpacing);

                        row.RelativeItem(0.3f).Element(c =>
                        {
                            c.Background("#7EB1A3") // rgb(126 177 163)
                                .Padding(10)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Name")
                                .FontColor(Colors.White)
                                .Bold();
                        });

                        row.RelativeItem(0.7f).Element(c =>
                        {
                            c.Background("#7EB1A3")
                                .Padding(10)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Value")
                                .FontColor(Colors.White)
                                .Bold();
                        });
                    });

                    // Data rows
                    foreach (var kvp in _data.ProjectOverview ?? new Dictionary<string, string>())
                    {
                        column.Item().Row(row =>
                        {
                            row.Spacing(cellSpacing);

                            // Name (30%)
                            row.RelativeItem(0.3f).Element(c =>
                            {
                                c.Background("#D3ECEB") // rgb(211 236 235)
                                    .Padding(10)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(kvp.Key ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });

                            // Value (70%)
                            row.RelativeItem(0.7f).Element(c =>
                            {
                                c.Background("#D3ECEB")
                                    .Padding(10)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(kvp.Value ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });
                        });
                    }
                });
            });

            // -------- Page 3: Tasks Section --------
            container.Page(page =>
            {
                page.MarginVertical(40);
                page.MarginHorizontal(10); // less horizontal margin
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    column.Spacing(2.5f);

                    // Title
                    column.Item().AlignCenter().Text("Tasks Section")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    // Header Row (5 columns)
                    column.Item().Row(row =>
                    {
                        row.Spacing(5);

                        string headerBg = "#7EB1A3"; // greenish tone
                        string headerTextColor = Colors.White;

                        // Task
                        row.RelativeItem(0.18f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Task")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Description
                        row.RelativeItem(0.28f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Description")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Deliverable Type
                        row.RelativeItem(0.18f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Deliverable Type")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Due Date
                        row.RelativeItem(0.18f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Due Date")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Status
                        row.RelativeItem(0.18f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Status")
                                .FontColor(headerTextColor)
                                .Bold();
                        });
                    });

                    // Small spacing after header
                    column.Item().Height(6);

                    // Data Rows
                    foreach (var task in _data.Tasks ?? new List<TaskDto>())
                    {
                        var statusName = task.StatusName;

                        column.Item().Row(row =>
                        {
                            row.Spacing(5);
                            string bgColor = "#D3ECEB"; // same soft greenish background

                            // Task
                            row.RelativeItem(0.18f).Element(c =>
                            {
                                c.Background(bgColor)
                                    .Padding(8)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(task.Title ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });

                            // Description
                            row.RelativeItem(0.28f).Element(c =>
                            {
                                c.Background(bgColor)
                                    .Padding(8)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(task.Description ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });

                            // Deliverable Type
                            row.RelativeItem(0.18f).Element(c =>
                            {
                                c.Background(bgColor)
                                    .Padding(8)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(task.DeliverableType ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });

                            // Due Date
                            row.RelativeItem(0.18f).Element(c =>
                            {
                                c.Background(bgColor)
                                    .Padding(8)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(task.EndDate?.ToString("MM/dd/yy") ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });

                            // Status
                            row.RelativeItem(0.18f).Element(c =>
                            {
                                c.Background(bgColor)
                                    .Padding(8)
                                    .MinHeight(28)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(statusName)
                                    .FontColor(Colors.Black)
                                    .FontSize(11)
                                    .Bold();
                            });
                        });

                        // vertical spacing between rows
                        column.Item().Height(6);
                    }
                });
            });

            // -------- Page X: Stakeholders Section --------
            container.Page(page =>
            {
                page.MarginVertical(40);
                page.MarginHorizontal(10); // less horizontal margin (like Tasks)
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    column.Spacing(2.5f);

                    // Title
                    column.Item().AlignLeft().Text("Stakeholders")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    // Header Row (Position | Name | Email)
                    column.Item().Row(row =>
                    {
                        row.Spacing(5);

                        string headerBg = "#7EB1A3"; // same greenish tone
                        string headerTextColor = Colors.White;

                        // Position
                        row.RelativeItem(0.25f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Position")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Name
                        row.RelativeItem(0.35f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Name")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        // Email
                        row.RelativeItem(0.4f).Element(c =>
                        {
                            c.Background(headerBg)
                                .Padding(8)
                                .MinHeight(30)
                                .AlignMiddle()
                                .AlignCenter()
                                .Text("Email")
                                .FontColor(headerTextColor)
                                .Bold();
                        });
                    });

                    // Small spacing after header
                    column.Item().Height(6);

                    // Data Rows
                    foreach (var stakeholder in _data.StakeHolders ?? new List<StakeHolderDto>())
                    {
                        // Project Manager Row
                        if (stakeholder.ProjectManageer != null)
                        {
                            column.Item().Row(row =>
                            {
                                row.Spacing(5);
                                string bgColor = "#D3ECEB"; // same soft greenish background

                                row.RelativeItem(0.25f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text("Project Manager")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });

                                row.RelativeItem(0.35f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text(stakeholder.ProjectManageer.Name ?? "-")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });

                                row.RelativeItem(0.4f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text(stakeholder.ProjectManageer.Email ?? "-")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });
                            });

                            column.Item().Height(6);
                        }

                        // Sub Contractor Row(s)
                        if (stakeholder.Subcontractor != null)
                        {
                            column.Item().Row(row =>
                            {
                                row.Spacing(5);
                                string bgColor = "#D3ECEB";

                                row.RelativeItem(0.25f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text("Sub Contractor")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });

                                row.RelativeItem(0.35f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text(stakeholder.Subcontractor.Name ?? "-")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });

                                row.RelativeItem(0.4f).Element(c =>
                                {
                                    c.Background(bgColor)
                                        .Padding(8)
                                        .MinHeight(28)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Text(stakeholder.Subcontractor.Email ?? "-")
                                        .FontColor(Colors.Black)
                                        .FontSize(11)
                                        .Bold();
                                });
                            });

                            column.Item().Height(6);
                        }
                    }

                    // Empty State
                    if ((_data.StakeHolders == null) || !_data.StakeHolders.Any())
                    {
                        column.Item().AlignCenter().PaddingVertical(10)
                            .Text("No stakeholders found.")
                            .Italic()
                            .FontColor(Colors.Grey.Medium);
                    }
                });
            });


            var groupedEntries = _data.ProjectBudgetEntries.GroupBy(x => new { x.CategoryId, x.CategoryName })
                                        .ToList();

            // -------- Page: Budget Overview --------
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // 🔹 Section Title
                    column.Item().AlignLeft().Text("Budget Overview")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    // 🔹 Budget Cards Row
                    column.Item().Row(row =>
                    {
                        row.Spacing(15);

                        // Shared styles
                        string cardBorderColor = "#C9C9C9";
                        string cardBg = Colors.White;
                        string iconColor = "#7EB1A3"; // same green tone

                        // Card 1 - Approved Budget
                        row.RelativeItem(1).Element(cardContainer =>
                        {
                            cardContainer
                                .Background(cardBg)
                                .Border(1)
                                .BorderColor(cardBorderColor)
                                .CornerRadius(10)
                                .Padding(15)
                                .Column(card =>
                                {
                                    card.Spacing(8);

                                    card.Item().Row(inner =>
                                    {
                                        inner.ConstantItem(24).Text("") // database icon
                                            .FontFamily("Segoe MDL2 Assets")
                                            .FontSize(16)
                                            .FontColor(iconColor);

                                        inner.RelativeItem().AlignLeft().Text("Approved Budget")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });

                                    card.Item().Text(_data.Project.TotalApprovedBudget.HasValue
                                        ? $"${_data.Project.TotalApprovedBudget.Value:N2}"
                                        : "-")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                });
                        });

                        // Card 2 - Disbursed Budget
                        row.RelativeItem(1).Element(cardContainer =>
                        {
                            cardContainer
                                .Background(cardBg)
                                .Border(1)
                                .BorderColor(cardBorderColor)
                                .CornerRadius(10)
                                .Padding(15)
                                .Column(card =>
                                {
                                    card.Spacing(8);

                                    card.Item().Row(inner =>
                                    {
                                        inner.ConstantItem(24).Text("") // chart icon
                                            .FontFamily("Segoe MDL2 Assets")
                                            .FontSize(16)
                                            .FontColor(iconColor);

                                        inner.RelativeItem().AlignLeft().Text("Disbursed Budget")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });

                                    card.Item().Text(_data.Project.TotalDisbursedBudget.HasValue
                                        ? $"${_data.Project.TotalDisbursedBudget.Value:N2}"
                                        : "-")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                });
                        });

                        // Card 3 - Remaining Budget
                        row.RelativeItem(1).Element(cardContainer =>
                        {
                            cardContainer
                                .Background(cardBg)
                                .Border(1)
                                .BorderColor(cardBorderColor)
                                .CornerRadius(10)
                                .Padding(15)
                                .Column(card =>
                                {
                                    card.Spacing(8);

                                    card.Item().Row(inner =>
                                    {
                                        inner.ConstantItem(24).Text("") // user icon
                                            .FontFamily("Segoe MDL2 Assets")
                                            .FontSize(16)
                                            .FontColor(iconColor);

                                        inner.RelativeItem().AlignLeft().Text("Remaining Budget")
                                            .FontSize(12)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });

                                    card.Item().Text(_data.Project.TotalRemainingBudget.HasValue
                                        ? $"${_data.Project.TotalRemainingBudget.Value:N2}"
                                        : "-")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                });
                        });
                    });

                    // --- Spacing before the next section ---
                    //column.Item().Height(10);

                    // ------- Budget Category Card --------
                    // Wrap inside an element so corner radius applies properly

                    

                    column.Item().Element(container =>
                    {
                        container
                            .Background(Colors.White)
                            .CornerRadius(10)
                            .Border(1)
                            .BorderColor("#CCCCCC")
                            .Padding(20)
                            .Column(card =>
                            {
                                // Title
                                card.Item().Text("Budget Category")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Colors.Black);

                                // Header row
                                card.Item().Row(row =>
                                {
                                    row.Spacing(5);
                                    string headerBg = "#FFFFFF";
                                    string headerTextColor = "#B0B0B0";

                                    row.RelativeItem(0.5f).Element(c =>
                                    {
                                        c.Background(headerBg)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignLeft()
                                            .Text("Category Name")
                                            .Bold()
                                            .FontColor(headerTextColor);
                                    });

                                    row.RelativeItem(0.166f).Element(c =>
                                    {
                                        c.Background(headerBg)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text("Approved Amount")
                                            .Bold()
                                            .FontColor(headerTextColor);
                                    });

                                    row.RelativeItem(0.166f).Element(c =>
                                    {
                                        c.Background(headerBg)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text("Disbursed Amount")
                                            .Bold()
                                            .FontColor(headerTextColor);
                                    });

                                    row.RelativeItem(0.166f).Element(c =>
                                    {
                                        c.Background(headerBg)
                                            .Padding(8)
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text("Remaining Amount")
                                            .Bold()
                                            .FontColor(headerTextColor);
                                    });
                                });

                                card.Item().LineHorizontal(1).LineColor("#B0B0B0");

                                // Data rows (same logic as before)
                                var entries = _data.ProjectBudgetEntries ?? new List<ProjectBudgetEntryDto>();

                                if (entries.Any())
                                {
                                   

                                    foreach (var group in groupedEntries)
                                    {
                                        decimal approved = group.FirstOrDefault(x => x.TypeId == 1)?.Amount ?? 0;
                                        decimal disbursed = group.FirstOrDefault(x => x.TypeId == 2)?.Amount ?? 0;
                                        decimal remaining = group.FirstOrDefault(x => x.TypeId == 3)?.Amount ?? 0;

                                        card.Item().Row(row =>
                                        {
                                            row.Spacing(5);

                                            row.RelativeItem(0.5f).Element(c =>
                                            {
                                                c.Padding(8)
                                                 .AlignMiddle()
                                                 .AlignLeft()
                                                 .Text(group.Key.CategoryName ?? "-")
                                                 .FontSize(11)
                                                 .FontColor(Colors.Black);
                                            });

                                            row.RelativeItem(0.166f).Element(c =>
                                            {
                                                c.Padding(8)
                                                 .AlignMiddle()
                                                 .AlignCenter()
                                                 .Text($"${approved:N2}")
                                                 .FontSize(11)
                                                 .FontColor(Colors.Black);
                                            });

                                            row.RelativeItem(0.166f).Element(c =>
                                            {
                                                c.Padding(8)
                                                 .AlignMiddle()
                                                 .AlignCenter()
                                                 .Text($"${disbursed:N2}")
                                                 .FontSize(11)
                                                 .FontColor(Colors.Black);
                                            });

                                            row.RelativeItem(0.166f).Element(c =>
                                            {
                                                c.Padding(8)
                                                 .AlignMiddle()
                                                 .AlignCenter()
                                                 .Text($"${remaining:N2}")
                                                 .FontSize(11)
                                                 .FontColor(Colors.Black);
                                            });
                                        });

                                        card.Item().LineHorizontal(0.8f).LineColor("#B0B0B0");
                                    }
                                }
                                else
                                {
                                    card.Item().AlignCenter().PaddingVertical(20)
                                        .Text("No budget entries available.")
                                        .FontSize(12)
                                        .Italic()
                                        .FontColor(Colors.Grey.Darken2);
                                }
                            });
                    });
                });
            });

            // ---------- Per-category pages: Disbursement Logs ----------
            

            const float cellSpacing = 6f;
            string cardBorderColor = "#C9C9C9";
            string cardBg = Colors.White;
            string iconColor = "#7EB1A3";
            string headerBg = "#ffffff";
            string dividerColor = "#B0B0B0";

            foreach (var group in groupedEntries)
            {
                var categoryName = group.Key.CategoryName ?? "Unnamed Category";

                // compute category-level totals (TypeId: 1=Approved,2=Disbursed,3=Remaining)
                var approved = group.FirstOrDefault(x => x.TypeId == 1)?.Amount ?? 0m;
                var disbursed = group.FirstOrDefault(x => x.TypeId == 2)?.Amount ?? 0m;
                var remaining = group.FirstOrDefault(x => x.TypeId == 3)?.Amount ?? 0m;

                // collect all disbursements for this category (safely)
                var disbursements = group
                    .SelectMany(e => e.Disbursements ?? new List<DisbursementDto>())
                    .OrderByDescending(d => d.DisbursementDate)
                    .ToList();

                // one page per category
                container.Page(page =>
                {
                    page.MarginVertical(40);
                    page.MarginHorizontal(10);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);

                    page.Content().Column(column =>
                    {
                        column.Spacing(18);

                        // Title
                        column.Item().Text(categoryName)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Black);

                        // Cards Row (3)
                        column.Item().Row(row =>
                        {
                            row.Spacing(cellSpacing);

                            void AddCard(string title, decimal value)
                            {
                                row.RelativeItem(1).Element(cardContainer =>
                                {
                                    // ensure corner radius applies by wrapping element
                                    cardContainer.Element(e =>
                                    {
                                        e.Background(cardBg)
                                         .CornerRadius(10)
                                         .Border(1)
                                         .BorderColor(cardBorderColor)
                                         .Padding(12)
                                         .Column(card =>
                                         {
                                             card.Spacing(6);

                                             card.Item().Text(title)
                                                 .FontSize(12)
                                                 .Bold()
                                                 .FontColor(Colors.Grey.Darken3)
                                                 .AlignLeft();

                                             card.Item().Text(value != 0 ? $"${value:N2}" : "-")
                                                 .FontSize(14)
                                                 .Bold()
                                                 .FontColor(Colors.Black)
                                                 .AlignLeft();
                                         });
                                    });
                                });
                            }

                            AddCard("Approved Budget", approved);
                            AddCard("Disbursed Budget", disbursed);
                            AddCard("Remaining Budget", remaining);
                        });


                        // Disbursement logs table container (card style)
                        column.Item().Element(tableContainer =>
                        {
                            tableContainer
                                .Background(Colors.White)
                                .CornerRadius(10)
                                .Border(1)
                                .BorderColor("#CCCCCC")
                                .Padding(14)
                                .Column(tc =>
                                {
                                    tc.Spacing(8);

                                    // Table Title (optional inside card)
                                    tc.Item().Text("Disbursed Amount")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Black);

                                    // Header row
                                    tc.Item().Row(row =>
                                    {
                                        row.Spacing(cellSpacing);

                                        // Date (20%)
                                        row.RelativeItem(0.2f).Element(c =>
                                        {
                                            c.Background(headerBg)
                                             .Padding(8)
                                             .MinHeight(30)
                                             .AlignMiddle()
                                             .AlignCenter()
                                             .Text("Date")
                                             .Bold()
                                             .FontColor(Colors.Black);
                                        });

                                        // Description (60%)
                                        row.RelativeItem(0.6f).Element(c =>
                                        {
                                            c.Background(headerBg)
                                             .Padding(8)
                                             .MinHeight(30)
                                             .AlignMiddle()
                                             .AlignLeft()
                                             .Text("Description")
                                             .Bold()
                                             .FontColor(Colors.Black);
                                        });

                                        // Amount (20%)
                                        row.RelativeItem(0.2f).Element(c =>
                                        {
                                            c.Background(headerBg)
                                             .Padding(8)
                                             .MinHeight(30)
                                             .AlignMiddle()
                                             .AlignCenter()
                                             .Text("Amount")
                                             .Bold()
                                             .FontColor(Colors.Black);
                                        });
                                    });

                                    // header divider
                                    tc.Item().LineHorizontal(1).LineColor(dividerColor);

                                    // Data rows
                                    if (disbursements.Any())
                                    {
                                        foreach (var d in disbursements)
                                        {
                                            tc.Item().Row(row =>
                                            {
                                                row.Spacing(cellSpacing);

                                                // Date
                                                row.RelativeItem(0.2f).Element(c =>
                                                {
                                                    c.Padding(8)
                                                     .MinHeight(26)
                                                     .AlignMiddle()
                                                     .AlignCenter()
                                                     .Text(d.DisbursementDate.ToString("MM/dd/yy"))
                                                     .FontSize(11)
                                                     .FontColor(Colors.Black);
                                                });

                                                // Description
                                                row.RelativeItem(0.6f).Element(c =>
                                                {
                                                    c.Padding(8)
                                                     .MinHeight(26)
                                                     .AlignMiddle()
                                                     .AlignLeft()
                                                     .Text(d.Description ?? "-")
                                                     .FontSize(11)
                                                     .FontColor(Colors.Black);
                                                });

                                                // Amount (right aligned)
                                                row.RelativeItem(0.2f).Element(c =>
                                                {
                                                    c.Padding(8)
                                                     .MinHeight(26)
                                                     .AlignMiddle()
                                                     .AlignRight()
                                                     .Text(d.DisbursedAmount != 0 ? $"${d.DisbursedAmount:N2}" : "-")
                                                     .FontSize(11)
                                                     .FontColor(Colors.Black);
                                                });
                                            });

                                            // row divider
                                            tc.Item().LineHorizontal(0.8f).LineColor(dividerColor);
                                        }
                                    }
                                    else
                                    {
                                        tc.Item().PaddingVertical(8).AlignCenter().Text("No disbursement logs available.")
                                            .Italic()
                                            .FontColor(Colors.Grey.Darken2);
                                    }
                                });
                        });

                        // optional: small bottom spacing
                        column.Item().Height(6);
                    });
                });
            }

            container.Page(page =>
            {
                page.MarginVertical(40);
                page.MarginHorizontal(10);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Content().Column(column =>
                {
                    column.Spacing(2.5f);

                    // Title
                    column.Item().AlignLeft().Text("Claim Details")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Black);

                    // Header Row
                    column.Item().Row(row =>
                    {
                        row.Spacing(5);

                        string headerBg = "#7EB1A3"; // green header background
                        string headerTextColor = Colors.White;

                        row.RelativeItem(0.2f).Element(c =>
                        {
                            c.Background(headerBg).Padding(8).MinHeight(30)
                                .AlignMiddle().AlignCenter()
                                .Text("Claim Number")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        row.RelativeItem(0.2f).Element(c =>
                        {
                            c.Background(headerBg).Padding(8).MinHeight(30)
                                .AlignMiddle().AlignCenter()
                                .Text("Disbursed Amount")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        row.RelativeItem(0.2f).Element(c =>
                        {
                            c.Background(headerBg).Padding(8).MinHeight(30)
                                .AlignMiddle().AlignCenter()
                                .Text("Category")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        row.RelativeItem(0.25f).Element(c =>
                        {
                            c.Background(headerBg).Padding(8).MinHeight(30)
                                .AlignMiddle().AlignCenter()
                                .Text("Description")
                                .FontColor(headerTextColor)
                                .Bold();
                        });

                        row.RelativeItem(0.15f).Element(c =>
                        {
                            c.Background(headerBg).Padding(8).MinHeight(30)
                                .AlignMiddle().AlignCenter()
                                .Text("Date")
                                .FontColor(headerTextColor)
                                .Bold();
                        });
                    });

                    column.Item().Height(6);

                    // Flatten all disbursements from ProjectBudgetEntries
                    var allDisbursements = _data.ProjectBudgetEntries.SelectMany(entry => entry.Disbursements).ToList();

                    // Data Rows
                    foreach (var d in allDisbursements)
                    {
                        string bgColor = "#D3ECEB"; // soft green row background
                        column.Item().Row(row =>
                        {
                            row.Spacing(5);

                            row.RelativeItem(0.2f).Element(c =>
                            {
                                c.Background(bgColor).Padding(8).MinHeight(28)
                                    .AlignMiddle().AlignCenter()
                                    .Text(d.ClaimNumber?.ToString() ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11);
                            });

                            row.RelativeItem(0.2f).Element(c =>
                            {
                                c.Background(bgColor).Padding(8).MinHeight(28)
                                    .AlignMiddle().AlignCenter()
                                    .Text($"${d.DisbursedAmount:N2}")
                                    .FontColor(Colors.Black)
                                    .FontSize(11);
                            });

                            row.RelativeItem(0.2f).Element(c =>
                            {
                                c.Background(bgColor).Padding(8).MinHeight(28)
                                    .AlignMiddle().AlignCenter()
                                    .Text(d.CategoryName ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11);
                            });

                            row.RelativeItem(0.25f).Element(c =>
                            {
                                c.Background(bgColor).Padding(8).MinHeight(28)
                                    .AlignMiddle().AlignCenter()
                                    .Text(d.Description ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11);
                            });

                            row.RelativeItem(0.15f).Element(c =>
                            {
                                c.Background(bgColor).Padding(8).MinHeight(28)
                                    .AlignMiddle().AlignCenter()
                                    .Text(d.DisbursementDate.ToString("MM/dd/yy") ?? "-")
                                    .FontColor(Colors.Black)
                                    .FontSize(11);
                            });
                        });

                        column.Item().Height(6);
                    }

                    // Empty State
                    if (allDisbursements.Count == 0)
                    {
                        column.Item().AlignCenter().PaddingVertical(10)
                            .Text("No claim details found.")
                            .Italic()
                            .FontColor(Colors.Grey.Medium);
                    }
                });
            });


        }


        private string GetLogoPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
        }
    }
}
