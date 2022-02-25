// Dan J Tarazi

using UnityEngine;
using Unity.Entities;

public class CenterOfMass : MonoBehaviour
{
public Vector3 middle;
	protected void Start()
	{
		 Mesh mesh = new Mesh();
		 mesh = GetComponent<MeshFilter>().mesh;

		middle = Vector3.zero;

		Vector3 a, b, c, ac, bc, w;
		float area = 0.0f;
		float totalarea = 0.0f;
		for (var i = 0; i < mesh.triangles.Length; i+=3)
		{
			a = mesh.vertices[mesh.triangles[i+0]];
			b = mesh.vertices[mesh.triangles[i+1]];
			c = mesh.vertices[mesh.triangles[i+2]];

			ac = a-c;
			bc = b-c;

			w = (a+b+c)/3.0f;

			area = (Vector3.Cross(ac, bc)).magnitude/2.0f;

			totalarea += area;
			middle += w*area;
		}

		middle /= totalarea;
	}
}