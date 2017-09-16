using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

namespace uHomography
{

[ExecuteInEditMode]
public class Homography : MonoBehaviour
{
    const string VertexPrefabPath = "uHomography/Prefabs/Vertex";

    [SerializeField]
    KeyCode handleToggleKey = KeyCode.None;

    bool isHandlesVisible_ = true;

    [SerializeField]
    int layer = 31;

    [SerializeField, HideInInspector]
    Camera uiCamera;

    [SerializeField, HideInInspector]
    Shader shader;

    [SerializeField, HideInInspector]
    Material material;

    [SerializeField, HideInInspector]
    DraggableVertex v00;

    [SerializeField, HideInInspector]
    DraggableVertex v01;

    [SerializeField, HideInInspector]
    DraggableVertex v10;

    [SerializeField, HideInInspector]
    DraggableVertex v11;

    float[] homography_ = null;
    float[] invHomography_ = null;

    void CreateCameraIfNeeded()
    {
        if (uiCamera) return;

        var go = new GameObject("uHomography UI Camera");
        go.transform.localPosition = new Vector3(0f, 0f, -9999f);
        go.transform.SetParent(transform);

        uiCamera = go.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 5.5f;
        uiCamera.useOcclusionCulling = false;
        uiCamera.allowHDR = false;
        uiCamera.allowMSAA = false;
        uiCamera.nearClipPlane = 1f;
        uiCamera.farClipPlane = 100f;
        uiCamera.depth = 100;

        var raycaster = go.AddComponent<Physics2DRaycaster>();
        raycaster.eventMask = 1 << layer;
    }

    void CreateVertexIfNeeded(ref DraggableVertex vertex, Vector3 pos)
    {
        if (vertex || !uiCamera) return;

        var prefab = Resources.Load<GameObject>(VertexPrefabPath);
        Assert.IsNotNull(prefab, VertexPrefabPath + " was not found.");

        var go = Instantiate(prefab, uiCamera.transform);
        go.transform.localPosition = pos;
        vertex = go.GetComponent<DraggableVertex>();
        Assert.IsNotNull(vertex, "The Vertex prefab does not have DraggableVertex component.");

        vertex.camera = uiCamera;
    }

    void CreateMaterialIfNeeded()
    {
        if (!material)
        {
            if (!shader)
            {
                shader = Shader.Find("uHomography/Homography");
            }
            material = new Material(shader);
        }
    }

    float[] CalcHomographyMatrix()
    {
        var p00 = v00.viewPosition;
        var p01 = v01.viewPosition;
        var p10 = v10.viewPosition;
        var p11 = v11.viewPosition;

        var x00 = p00.x;
        var y00 = p00.y;
        var x01 = p01.x;
        var y01 = p01.y;
        var x10 = p10.x;
        var y10 = p10.y;
        var x11 = p11.x;
        var y11 = p11.y;

        var a = x10 - x11;
        var b = x01 - x11;
        var c = x00 - x01 - x10 + x11;
        var d = y10 - y11;
        var e = y01 - y11;
        var f = y00 - y01 - y10 + y11;

        var h13 = x00;
        var h23 = y00;
        var h32 = (c * d - a * f) / (b * d - a * e);
        var h31 = (c * e - b * f) / (a * e - b * d);
        var h11 = x10 - x00 + h31 * x10;
        var h12 = x01 - x00 + h32 * x01;
        var h21 = y10 - y00 + h31 * y10;
        var h22 = y01 - y00 + h32 * y01;

        return new float[] { h11, h12, h13, h21, h22, h23, h31, h32, 1f };
    }

    float[] CalcInverseMatrix(float[] mat)
    {
        var i11 = mat[0];
        var i12 = mat[1];
        var i13 = mat[2];
        var i21 = mat[3];
        var i22 = mat[4];
        var i23 = mat[5];
        var i31 = mat[6];
        var i32 = mat[7];
        var i33 = 1f;
        var a = 1f / (
            + (i11 * i22 * i33)
            + (i12 * i23 * i31)
            + (i13 * i21 * i32)
            - (i13 * i22 * i31)
            - (i12 * i21 * i33)
            - (i11 * i23 * i32)
        );

        var o11 = ( i22 * i33 - i23 * i32) / a;
        var o12 = (-i12 * i33 + i13 * i32) / a;
        var o13 = ( i12 * i23 - i13 * i22) / a;
        var o21 = (-i21 * i33 + i23 * i31) / a;
        var o22 = ( i11 * i33 - i13 * i31) / a;
        var o23 = (-i11 * i23 + i13 * i21) / a;
        var o31 = ( i21 * i32 - i22 * i31) / a;
        var o32 = (-i11 * i32 + i12 * i31) / a;
        var o33 = ( i11 * i22 - i12 * i21) / a;

        return new float[] { o11, o12, o13, o21, o22, o23, o31, o32, o33 };
    }

    void UpdateHomographyMatrix()
    {
        homography_ = CalcHomographyMatrix();
        invHomography_ = CalcInverseMatrix(homography_);
        v00.hasChanged = v01.hasChanged = v10.hasChanged = v11.hasChanged = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(handleToggleKey))
        {
            isHandlesVisible_ = !isHandlesVisible_;
            v00.gameObject.SetActive(isHandlesVisible_);
            v01.gameObject.SetActive(isHandlesVisible_);
            v10.gameObject.SetActive(isHandlesVisible_);
            v11.gameObject.SetActive(isHandlesVisible_);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateCameraIfNeeded();
        uiCamera.cullingMask = 1 << layer;

        CreateVertexIfNeeded(ref v00, new Vector3(-5f, -5f, 10f));
        CreateVertexIfNeeded(ref v01, new Vector3(-5f,  5f, 10f));
        CreateVertexIfNeeded(ref v10, new Vector3( 5f, -5f, 10f));
        CreateVertexIfNeeded(ref v11, new Vector3( 5f,  5f, 10f));
        v00.gameObject.layer = layer;
        v01.gameObject.layer = layer;
        v10.gameObject.layer = layer;
        v11.gameObject.layer = layer;

        CreateMaterialIfNeeded();

        bool isHandleMoved = (v00.hasChanged || v01.hasChanged || v10.hasChanged || v11.hasChanged);
        bool isNotInitialized =
            (homography_ == null) || (homography_.Length == 0) ||
            (invHomography_ == null) || (invHomography_.Length == 0);
        if (isHandleMoved || isNotInitialized)
        {
            UpdateHomographyMatrix();
        }

        material.SetFloatArray("_Homography", homography_);
        material.SetFloatArray("_InvHomography", invHomography_);
        Graphics.Blit(source, destination, material);
    }

    public Vector2 GetHomographyPosition(Vector2 pos)
    {
        float s =  homography_[6] * pos.x + homography_[7] * pos.y + homography_[8];
        float x = (homography_[0] * pos.x + homography_[1] * pos.y + homography_[2]) / s;
        float y = (homography_[3] * pos.x + homography_[4] * pos.y + homography_[5]) / s;
        return new Vector2(x, y);
    }

    public Vector2 GetInverseHomographyPosition(Vector2 pos)
    {
        float s =  invHomography_[6] * pos.x + invHomography_[7] * pos.y + invHomography_[8];
        float x = (invHomography_[0] * pos.x + invHomography_[1] * pos.y + invHomography_[2]) / s;
        float y = (invHomography_[3] * pos.x + invHomography_[4] * pos.y + invHomography_[5]) / s;
        return new Vector2(x, y);
    }
}

}
