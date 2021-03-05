#define MAX_INSTANCE_COUNT_IN_COMBINED_MESH 900
int _TotalInstanceCountInCombinedMesh;

int UVToMeshInstanceInsideCombinedMesh (float2 uv)
{
    // uv = float2(0.5, 0.5);
    float lutDimension = ceil(sqrt((float)_TotalInstanceCountInCombinedMesh));
    float shift = (1.0 / lutDimension) * 0.1;
    return floor((uv.x + shift) * lutDimension) + floor((uv.y + shift) * lutDimension) * lutDimension;
    //return _TotalInstanceCountInCombinedMesh;
}