using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointCloud : MonoBehaviour
{
	public float particleSize = 10f;
	private int currentResolution;
	private ParticleSystem.Particle[] points;
	private List<Vector3> realParticles = new List<Vector3> ();

	void Start ()
	{
		ZigInput.Instance.AddListener (gameObject);    
	}
	
	void Zig_Update (ZigInput input)
	{
		short[] rawDepthMap = ZigInput.Depth.data;
		short[] rawLabelMap = ZigInput.LabelMap.data;
		
		int width = ZigInput.Depth.xres;
		int height = ZigInput.Depth.yres;
		
		realParticles.Clear ();
		

	
		
		for (int i=0; i<width; i+=3) {
			for (int j=0; j<height; j+=3) {
				if (rawDepthMap [j * width + i] != 0&&rawDepthMap[j*width+i]<2500) {
					Vector3 image = new Vector3 (i, j, rawDepthMap [j * width + i]);
					Vector3 real = ZigInput.ConvertImageToWorldSpace (image);
					real.x = real.x;
					real.y = real.y;
					real.z = real.z;
					realParticles.Add (real);
				}	
			}
		}
		points = new ParticleSystem.Particle[realParticles.Count];
			
		for (int i = 0; i < realParticles.Count; i++) {

			points [i].position = realParticles [i];
			
			points [i].color = new Color (255f, 255f, 0f);
			points [i].size = particleSize;
		}
		
//		particleSystem.SetParticles (points, points.Length);
	}
	
	public ParticleSystem.Particle[] getPoints ()
	{
		return points;
	}
}