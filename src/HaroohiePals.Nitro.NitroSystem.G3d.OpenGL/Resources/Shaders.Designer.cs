﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HaroohiePals.Nitro.NitroSystem.G3d.OpenGL.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Shaders {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Shaders() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HaroohiePals.Nitro.NitroSystem.G3d.OpenGL.Resources.Shaders", typeof(Shaders).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 400 core
        ///
        ///layout (location = 0) out vec4 FragColor;
        ///layout (location = 1) out uvec4 outPickingId;
        ///layout (location = 2) out uint outFogBit;
        ///
        ///in vec4 vertexColor; // the input variable from the vertex shader (same name and same type)
        ///in vec2 texCoord;
        ///
        ///uniform sampler2D texture0;
        ///uniform uint pickingId;
        ///
        ///layout (std140) uniform uboData
        ///{
        ///    uint polygonAttr;
        ///    uint texImageParam;
        ///    uint flags;
        ///    uint padding;
        ///    vec4 diffuse;
        ///    vec4 ambient;
        ///    vec4 specular;
        ///    vec4 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string G3dModelFragment {
            get {
                return ResourceManager.GetString("G3dModelFragment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 400 core
        ///
        ///layout (location = 0) in vec3 aPosition;
        ///layout (location = 1) in vec3 aNormalOrColor;
        ///layout (location = 2) in vec2 aTexCoord;
        ///layout (location = 3) in uint aMtxId;
        ///
        ///out vec4 vertexColor; // specify a color output to the fragment shader
        ///out vec2 texCoord;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///layout (std140) uniform uboData
        ///{
        ///    uint polygonAttr;
        ///    uint texImageParam;
        ///    uint flags;
        ///    uint padding;
        ///    vec4 diffuse;
        ///    vec4 ambient; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string G3dModelVertex {
            get {
                return ResourceManager.GetString("G3dModelVertex", resourceCulture);
            }
        }
    }
}
