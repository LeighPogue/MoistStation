﻿using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared._Harmony.ReadyManifest;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Harmony.ReadyManifest.UI;

[GenerateTypedNameReferences]
public sealed partial class ReadyManifestUi : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public ReadyManifestUi()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    public void RebuildUI(IReadOnlyDictionary<ProtoId<JobPrototype>, ReadyManifestJobData> jobCounts)
    {
        ReadyManifestListing.RemoveAllChildren();

        var departments = new List<DepartmentPrototype>();

        foreach (var department in _prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
        {
            if (department.EditorHidden)
                continue;

            departments.Add(department);
        }

        departments.Sort(DepartmentUIComparer.Instance);

        foreach (var department in departments)
        {
            var departmentName = Loc.GetString(department.Name);

            var departmentControl = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
                Name = department.ID,
                ToolTip = Loc.GetString("humanoid-profile-editor-jobs-amount-in-department-tooltip",
                    ("departmentName", departmentName)),
            };

            departmentControl.AddChild(new Label
            {
                StyleClasses = { "LabelBig" },
                Text = departmentName,
            });

            var jobs = department.Roles.Select(jobId => _prototypeManager.Index(jobId))
                .Where(job => job.SetPreference)
                .ToArray();

            Array.Sort(jobs, JobUIComparer.Instance);

            foreach (var job in jobs)
            {
                var readyCount = jobCounts.GetValueOrDefault(job);

                var jobControl = new ReadyManifestJobListing(job, readyCount);
                departmentControl.AddChild(jobControl);
            }

            ReadyManifestListing.AddChild(departmentControl);
        }
    }
}
