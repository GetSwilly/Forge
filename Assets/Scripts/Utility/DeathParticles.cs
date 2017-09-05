using UnityEngine;
using System.Collections;

public class DeathParticles : MonoBehaviour {

	public AudioClip deathSound;
	ParticleSystem _particles;

	void Awake () {
		_particles = this.GetComponent<ParticleSystem>();
	}

	void OnEnable(){
		StopAllCoroutines();
		StartCoroutine(PlayParticles());
	}

	IEnumerator PlayParticles(){
		_particles.Clear();
		_particles.Stop();
		_particles.Clear();

		float time = _particles.main.duration;

		_particles.time = 0;
		_particles.Play();

		AudioSource _audio = GetComponent<AudioSource>();

		if(deathSound != null && _audio != null){
			_audio.Stop();
			_audio.PlayOneShot(deathSound);
		}

		yield return new WaitForSeconds(time);

		_particles.Stop();

		this.gameObject.SetActive(false);
	}
}
