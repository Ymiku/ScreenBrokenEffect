using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class AreaInfo
{
	private List<Vector2> area;
	private Transform trans;
	private SpriteRenderer sprite;
	private Vector3 center;
	private Vector2 basePoint;
	private Vector2 speed;
	private Vector2 size;
	private float rotate;
}
public class BrokenScreen : MonoBehaviour {
	public Camera seeCamera;
	public Camera[] cameraToRender;
	public Texture2D brokenRefer;
	private delegate IEnumerator AfterBroken();
	private Texture2D _brokenReferAfter;
	private Texture2D _shot;
	private float _spriteScale;
	private List<Color> _colorList = new List<Color>();
	private Dictionary<Color,List<Vector2>> _colorToArea = new Dictionary<Color, List<Vector2>>();
	private Transform[] _transArray;
	private SpriteRenderer[] _spriteArray;
	private Vector3[] _centerArray;
	private Vector2[] _basePointArray;
	private Vector2[] _sizeArray;
	private Vector2[] _speedArray;
	private float[] _rotateArray;

	private int _screenWidth;
	private int _screenHeight;
	private float _time = 0f;
	[SerializeField]
	private bool _isReady = false;
	// Use this for initialization
	void Awake () {
		Init ();
		PrepareToBroken ();
		Broken ();
	}
	void Init()
	{
		_screenWidth = Screen.width;
		_screenHeight = Screen.height;

		_spriteScale = (seeCamera.ViewportToWorldPoint (new Vector3 (1f, 0f, seeCamera.nearClipPlane + 1f)).x - seeCamera.ViewportToWorldPoint (new Vector3 (0f, 0f, seeCamera.nearClipPlane +1f)).x)
			/_screenWidth*100;
		_brokenReferAfter = ScaleTexture (brokenRefer, _screenWidth, _screenHeight);
		Color[] referColorArray = _brokenReferAfter.GetPixels ();
	
		for (int i = 0; i < referColorArray.Length; i++) {
			if (!_colorToArea.ContainsKey (referColorArray [i])) {
				_colorList.Add (referColorArray[i]);
				_colorToArea.Add (referColorArray [i],new List<Vector2>());
			}
			_colorToArea [referColorArray [i]].Add (CountToVector2 (i,_screenWidth,_screenHeight));
		}
		_speedArray = new Vector2[_colorList.Count];
		_rotateArray = new float[_colorList.Count];
		_transArray = new Transform[_colorList.Count];
		_spriteArray = new SpriteRenderer[_colorList.Count];
		_centerArray = new Vector3[_colorList.Count];
		_basePointArray = new Vector2[_colorList.Count];
		_sizeArray = new Vector2[_colorList.Count];
		for (int i = 0; i < _speedArray.Length; i++) {
			_speedArray [i] = new Vector2 (Random.Range (-4f, 4f), Random.Range (0, 5f));
			_rotateArray [i] = _speedArray [i].x * -100f;
		}
		for (int i = 0; i < _colorList.Count; i++) {
			CreatSprite(i,_colorToArea[_colorList[i]]);
		}
	}
	void CreatSprite(int count,List<Vector2> area)
	{
		if (area.Count <= 0)
			return;
		int minX = (int)area[0].x;
		int minY = (int)area[0].y;
		int maxX = (int)area[0].x;
		int maxY = (int)area[0].y;
		for (int i = 1; i < area.Count; i++) {
			if(area[i].x<minX)minX = (int)area[i].x;
			if(area[i].x>maxX)maxX = (int)area[i].x;
			if(area[i].y<minY)minY = (int)area[i].y;
			if(area[i].y>maxY)maxY = (int)area[i].y;
		}
		int textureWidth = maxX - minX + 1;
		int textureHeight = maxY - minY + 1;
		Vector2 center = new Vector2 ((maxX + minX) / 2f, (maxY + minY) / 2f);
		Vector3 worldPos = seeCamera.ViewportToWorldPoint (new Vector3 (center.x / _screenWidth, center.y / _screenHeight, seeCamera.nearClipPlane + 1));
		_centerArray [count] = seeCamera.transform.InverseTransformPoint (worldPos);
		Color[] initColor = new Color[textureWidth*textureHeight];
		for (int i = 0; i < initColor.Length; i++) {
			initColor [i] = new Color (0f,0f,0f,0f);
		}
		Texture2D spriteTexture = new Texture2D(textureWidth,textureHeight,TextureFormat.ARGB32,false);
		spriteTexture.SetPixels (initColor);
		GameObject spriteObj = new GameObject ();
		spriteObj.transform.localScale = new Vector3 (_spriteScale,_spriteScale,1f);
		spriteObj.transform.SetParent (seeCamera.transform);
		SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer> ();
		sr.sortingOrder = 100;
		_transArray [count] = spriteObj.transform;
		_spriteArray [count] = sr;
		_basePointArray [count] = new Vector2 (minX, minY);
		_sizeArray [count] = new Vector2 (textureWidth, textureHeight);
		spriteObj.SetActive (false);
	}
	Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight)
	{
		Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32,false);  
		int w;
		int h;
		float xScale = 1920f/targetWidth;
		float yScale = 1080f/targetHeight;
		Color[] sourceColor = source.GetPixels();
		Color[] resultColor = new Color[targetWidth*targetHeight];
		for (int i = 0; i < targetHeight; i++) {
			for (int j = 0; j < targetWidth; j++) {
				w = (int)(j * xScale);
				h = (int)(i * yScale);
				resultColor[i*targetWidth+j] = sourceColor[h*1920+w];
			}
		}
		result.SetPixels (resultColor);
		result.Apply ();
		return result;
	}
	Vector2 CountToVector2(int count,int width,int height)
	{
		return new Vector2((int)(count%width),(int)(count/width));
	}
	int VectorToCount(Vector2 v,float width,float height)
	{
		return VectorToCount (v,(int)width,(int)height);
	}
	int VectorToCount(Vector2 v,int width,int height)
	{
		if (v.x < 0 || v.y < 0)
			return -1;
		if (v.x >= width || v.y >= height)
			return -1;
		return (int)(v.x + v.y * width);
	}
	public void PrepareToBroken()
	{
		_isReady = false;
		CancelInvoke ();
		_shot = CaptureScreenshot2( new Rect( 0,0,_screenWidth,_screenHeight)); 
		Color[] shotColor = _shot.GetPixels();
		for (int i = 0; i < _colorList.Count; i++) {
			_transArray[i].gameObject.SetActive (true);
			_transArray[i].transform.localPosition = _centerArray[i];
			_transArray[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
			_speedArray [i] = new Vector2 (Random.Range (0f, 4f)*(_transArray[i].localPosition.x>0?1f:-1f), Random.Range (0, 5f));
			_rotateArray [i] = -_speedArray [i].x;

		}
		for (int i = 0; i < _colorList.Count; i++) {
			Color c = _colorList[i];
			Vector2 basePoint = _basePointArray[i];
			List<Vector2> area = _colorToArea [c];
			Vector2 size = _sizeArray[i];
			Texture2D t = new Texture2D((int)size.x,(int)size.y,TextureFormat.ARGB32,false);
			Color[] areaColor = new Color[(int)(size.x*size.y)];
			for (int j = 0; j < area.Count; j++) {
				Vector2 offPos = area [j] - basePoint;
				areaColor [VectorToCount (offPos,size.x,size.y)] = shotColor [VectorToCount (area [j],_screenWidth,_screenHeight)];
			}
			t.SetPixels (areaColor);
			t.Apply ();
			_spriteArray[i].sprite = Sprite.Create (t,new Rect(0f,0f, (int)size.x,(int)size.y),new Vector2(0.5f,0.5f));
		}

	}
	public void Broken()
	{
		_isReady = true;
		_time = 0f;
		Invoke ("End",10f);
	}
	void End()
	{
		_isReady = false;
		for (int i = 0; i < _transArray.Length; i++) {
			_transArray [i].gameObject.SetActive (false);
		}
	}
	Texture2D CaptureScreenshot2(Rect rect)   
	{  
		RenderTexture rt = new RenderTexture ((int)rect.width, (int)rect.height, 0);
		Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32,false);  
		for (int i = 0; i < cameraToRender.Length; i++) {
			cameraToRender [i].targetTexture = rt;
			cameraToRender [i].Render ();
		}
		RenderTexture.active = rt;
		screenShot.ReadPixels(rect, 0, 0);  
		screenShot.Apply();
		for (int i = 0; i < cameraToRender.Length; i++) {
			cameraToRender [i].targetTexture = null;
		}
		RenderTexture.active = null;
		GameObject.Destroy (rt);
		return screenShot;
		//image.texture =rt;

	}  
	void BrokenUpdate()
	{
		Color c;
		Transform colorTrans;
		for (int i = 0; i < _colorList.Count; i++) {
			c = _colorList [i];
			colorTrans = _transArray[i];
			colorTrans.localPosition = new Vector3 (
				colorTrans.localPosition.x + _speedArray[i].x*Time.deltaTime,
				colorTrans.localPosition.y+_speedArray[i].y*Time.deltaTime-0.2f*_time*_time,
				colorTrans.localPosition.z
			);
			colorTrans.Rotate(0f,0f,_rotateArray[i]);
		}
	}
	void BeforeBrokenUpdate()
	{
		Color c;
		Transform colorTrans;
		float timeh;
		for (int i = 0; i < _colorList.Count; i++) {
			timeh = _time * 26f;
			c = _colorList [i];
			colorTrans = _transArray[i];
			colorTrans.localPosition = new Vector3 (
				colorTrans.localPosition.x - _speedArray[i].x*Time.deltaTime*0.5f*Mathf.Sin(timeh),
				colorTrans.localPosition.y-_speedArray[i].y*Time.deltaTime*0.5f*Mathf.Sin(timeh),
				colorTrans.localPosition.z
			);
			colorTrans.Rotate(0f,0f,-_rotateArray[i]*0.5f);
		}
	}
	void Update () {
		if (Input.GetKeyDown (KeyCode.Q)) {
			PrepareToBroken ();
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			Broken ();
		}
		if (_isReady) {
			if (_time < 0.2f) {
				if (_time < 0.06f) {
					BeforeBrokenUpdate ();
				}
			} else {
				BrokenUpdate ();
			}
			_time += Time.deltaTime;

		}
	}
}
