using UnityEngine;
using System.Collections;

public class AutoTransparent : MonoBehaviour {

    public enum TransparentComponent { };

    private Material[] _materials = null;
	private Shader[] oldShaders = null;
	private Color[] oldColors = null; //Color.black;
	private float targetTransparency = 0.3f;
	private float fadeTime = 0.5f;
	Renderer myRenderer;
	int multiplier;
	
	void Awake()
    {
		myRenderer = this.GetComponent<Renderer>();
	}

    public void BeTransparent(float _transparency, float _time)
    {
        targetTransparency = _transparency;
        fadeTime = _time;

        BeTransparent();
    }


	public void BeTransparent()
	{
		// reset the transparency;
        if (_materials == null)
		{
            _materials = myRenderer.materials;

            oldShaders = new Shader[_materials.Length];
            oldColors = new Color[_materials.Length];

            for (int i = 0; i < _materials.Length; i++)
            {
                // Save the current shader
                oldShaders[i] =  _materials[i].shader;
                oldColors[i] = _materials[i].color;

                Color C = oldColors[i];
                C.a -= 0.01f;


                _materials[i].color = C;

                _materials[i].shader = Shader.Find("Transparent/Diffuse");

                //_materials[i].shader.
            }
              
            // Save the current shader
            /*oldShaders = myRenderer.material.shader;
			oldColors = myRenderer.material.color;
			
			Color C = myRenderer.material.color;
			C.a = oldColors.a - 0.01f;
			myRenderer.material.color = C;
			
			myRenderer.material.shader = Shader.Find("Transparent/Diffuse");*/
		}
		
		multiplier = -1;
	}
	void Update()
	{

        bool shouldReset = true;
        for (int i = 0; i < _materials.Length; i++)
        {

            if (_materials[i].color.a < 1.0f)
            {
                shouldReset = false;
                _materials[i].color = Utilities.FadeColor(_materials[i].color, (1 / fadeTime) * Time.deltaTime * multiplier, targetTransparency, 1);
            }
        }


        if (shouldReset)
        {
            // Reset the shader

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].shader = oldShaders[i];
                _materials[i].color = oldColors[i];

            }


            // And remove this script
            Destroy(this);
        }
		multiplier = 1;
	}
}
