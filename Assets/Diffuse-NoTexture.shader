// colored vertex lighting
//originally from "Simple colored lighting"
//at https://docs.unity3d.com/Documentation/Components/SL-Shader.html
Shader "Vertex-NoTexture" {
    // a single color property
    Properties {
        _mainColor ("Main Color", Color) = (1,1,1,1)
        _ambColor ("Ambient Color", Color) = (.5,.5,.5,1)
        _emsColor ("Emission Color", Color) = (0,0,0,1)
        
    }
    // define one subshader
    SubShader {
    	Tags { "Queue" = "Transparent" }
        Pass {
            Material {
                Diffuse [_mainColor]
                Ambient[_ambColor]
                Emission[_emsColor]
            }
            Lighting On
            
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend OneMinusSrcAlpha One
        }
        //Pass {
    	//	Blend SrcAlpha OneMinusSrcAlpha
    	//}
    }
}