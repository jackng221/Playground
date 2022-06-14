using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    public GameObject trailPrefab;
    public SkinnedMeshRenderer meshRenderer;
    ParticleSystemRenderer pRenderer;
    [SerializeField] List<Mesh> meshList;

    public float spawnCD = 0.5f;
    [SerializeField] int currentMesh;

    private void Start()
    {
        currentMesh = 0;
        meshList = new List<Mesh>();
        for (int i = 0; i < 100; i++)
        {
            Mesh mesh = new Mesh();
            meshList.Add(mesh);
        }
        StartCoroutine("SpawnImage", 0f);
    }
    IEnumerator SpawnImage()
    {
        while (true)
        {
            meshRenderer.BakeMesh(meshList[currentMesh]);
            meshList[currentMesh] = mergedMesh(meshList[currentMesh]);

            GameObject trail = Instantiate(trailPrefab, gameObject.transform.position, gameObject.transform.rotation);
            pRenderer = trail.GetComponent<ParticleSystemRenderer>();

            pRenderer.mesh = meshList[currentMesh];

            if (currentMesh + 1 >= meshList.Count)
            {
                currentMesh = 0;
            }
            else
            {
                currentMesh++;
            }

            yield return new WaitForSeconds(spawnCD);
        }
    }

    //https://forum.unity.com/threads/combinemesh-with-submeshes.39119/
    //By Programmer_RM, Dec 5, 2016
    //returns the same mesh with all submeshes merged into one submesh
    static public Mesh mergedMesh(Mesh mesh)
    {
        Mesh merged = Object.Instantiate<Mesh>(mesh);
        int[] t = merged.triangles;
        merged.triangles = null;
        merged.subMeshCount = 1;
        merged.triangles = t;
        return merged;
    }
}
