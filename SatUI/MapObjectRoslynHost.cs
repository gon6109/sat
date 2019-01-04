using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SatUI
{
    class MapObjectRoslynHost : RoslynHost
    {

        public MapObjectRoslynHost(NuGetConfiguration nuGetConfiguration = null,
           IEnumerable<Assembly> additionalAssemblies = null,
           RoslynHostReferences references = null) : base(nuGetConfiguration, additionalAssemblies, references)
        {
        }

        protected override Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project previousProject = null)
        {
            var name = args.Name ?? "Program";
            var id = ProjectId.CreateNewId(name);

            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Script, languageVersion: LanguageVersion.Latest);

            compilationOptions = compilationOptions.WithScriptClassName(name);

            solution = solution.AddProject(ProjectInfo.Create(
                id,
                VersionStamp.Create(),
                name,
                name,
                LanguageNames.CSharp,
                isSubmission: true,
                parseOptions: parseOptions,
                hostObjectType: typeof(SatPlayer.MapObject),
                compilationOptions: compilationOptions,
                metadataReferences: previousProject != null ? ImmutableArray<MetadataReference>.Empty : DefaultReferences,
                projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null));
            var project = solution.GetProject(id);
            return project;
        }
    }
}
