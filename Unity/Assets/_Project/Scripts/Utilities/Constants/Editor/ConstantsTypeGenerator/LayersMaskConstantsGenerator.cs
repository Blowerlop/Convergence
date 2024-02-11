using System.CodeDom.Compiler;
using Project.Extensions;
using UnityEngine;

namespace _Project.ConstantGenerator
{
    public class LayersMaskConstantsGenerator : IConstantGenerator
    {
        public string className { get; set; } = "LayersMask";
        
        
        public void Write(IndentedTextWriter streamWriter)
        {
            foreach (string layer in GetLayers())
            {
                ConstantsGenerator.WriteSummary(streamWriter, layer);
                streamWriter.WriteLine($"public const int {layer.ConvertToValidIdentifier()} = {LayerMask.GetMask(layer)};");
            }
        }

        private string[] GetLayers()
        {
            return UnityEditorInternal.InternalEditorUtility.layers;
        }
    }
}