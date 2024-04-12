using System;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using Hachi.Data.Attributes;
using Hachi.Data.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Hachi.Data;

[Generator(LanguageNames.CSharp)]
public class JsonSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initialContext)
    {
        var attributesPipeline = initialContext.SyntaxProvider
            .ForAttributeWithMetadataName(fullyQualifiedMetadataName: typeof(JsonGeneratedAttribute).FullName!,
                (s, _) => s is RecordDeclarationSyntax,
                (ctx, _) => GetAttribute(ctx)).Where(x => x.HasValue).Collect();
        
        var jsonFilesPipeline = initialContext.AdditionalTextsProvider
            .Where(static a => a.Path.EndsWith(".json")).Collect();

        initialContext.RegisterSourceOutput(initialContext.CompilationProvider.Combine(attributesPipeline.Combine(jsonFilesPipeline)),
            (context, pair) =>
        {
            foreach (var attr in pair.Right.Left)
            {
                var attribute = attr!.Value.Item2;
                var file = pair.Right.Right.FirstOrDefault(x => x.Path.EndsWith(attribute.Source));
                if (file is null)
                    throw new FileNotFoundException($"File {attribute.Source} not found.");
                
                GenerateCode(context, pair.Left, attr!.Value.Item1, file.GetText()!.ToString());
            }
        });
    }
    
    private static (RecordDeclarationSyntax, JsonGeneratedAttribute)? GetAttribute(GeneratorAttributeSyntaxContext context)
    {
        foreach (var attributeSyntax in context.Attributes)
        {
            var value = attributeSyntax.ConstructorArguments.FirstOrDefault().Value;
            
            if (value is string source)
                return ((RecordDeclarationSyntax) context.TargetNode, new JsonGeneratedAttribute(source));
        }
        
        return null;
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation, 
        BaseTypeDeclarationSyntax classDeclaration, string source)
    {
        var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            throw new InvalidOperationException("Class symbol not found.");
        
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        
        var document = JsonDocument.Parse(source);
        var root = document.RootElement;
        var emitter = new JsonClassEmitter(namespaceName, classDeclaration.Identifier.Text);
        var obj = new JsonObject(root, emitter);
        emitter.Emit(classDeclaration.Identifier.Text, obj, true);
        
        context.AddSource($"{classDeclaration.Identifier}.g.cs", SourceText.From(emitter.GetResult(), Encoding.UTF8));
    }
}