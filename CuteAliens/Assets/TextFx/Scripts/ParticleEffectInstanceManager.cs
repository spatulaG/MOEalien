// A class to store all necessary information to handle the start, end and transitions of a particle effect within a TextFx animation
using UnityEngine;

[System.Serializable]
public class ParticleEffectInstanceManager
{
	ParticleEmitter m_emitter;
	EffectManager m_effect_manager_handle;
	float m_duration = 0;
	float m_delay = 0;
	Mesh m_letter_mesh;
	Vector3 m_position_offset;
	bool m_follow_mesh;
	bool m_active = false;
	Transform m_transform;
	
	public bool Active
	{
		get
		{
			return m_delay > 0 || m_duration > 0 || m_emitter.particleCount > 0;
		}
	}
	
	public ParticleEffectInstanceManager(ParticleEmitter p_emitter, EffectManager effect_manager, Mesh character_mesh, float delay, float duration, Vector3 position_offset, bool follow_mesh)
	{
		m_emitter = p_emitter;
		m_duration = duration;
		m_letter_mesh = character_mesh;
		m_delay = delay;
		m_position_offset = position_offset;
		m_follow_mesh = follow_mesh;
		m_effect_manager_handle = effect_manager;
		m_transform = m_emitter.transform;
		
		// Enable emitter
		m_emitter.emit = true;
		m_emitter.enabled = false;
		
		// Position effect according to offset
		m_letter_mesh.RecalculateNormals();
		if(!m_letter_mesh.normals[0].Equals(Vector3.zero))
		{
			Quaternion rotation = Quaternion.LookRotation(m_letter_mesh.normals[0], m_letter_mesh.vertices[1].Equals(m_letter_mesh.vertices[2]) ? Vector3.forward : m_letter_mesh.vertices[1] - m_letter_mesh.vertices[2]);
			m_transform.position = m_effect_manager_handle.m_transform.position + (rotation * m_position_offset) + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4;
			m_transform.rotation = rotation;
		}
		else
		{
			m_transform.position = m_effect_manager_handle.m_transform.position + m_position_offset + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4;
		}
	}
	
	// Updates particle effect. Returns true when effect is completely finished and ready to be reused.
	public bool Update(float delta_time)
	{
		if(!m_active)
		{
			if(m_delay > 0)
			{
				m_delay -= delta_time;
				if(m_delay < 0)
				{
					m_delay = 0;
				}
				
				return false;
			}
			
			m_active = true;
			m_emitter.emit = false;
			m_emitter.enabled = true;
			
			if(m_duration > 0)
			{
				m_emitter.emit = true;
			}
			else
			{
				m_emitter.Emit();
			}
		}
		
		if(m_follow_mesh)
		{
			// Position effect according to offset
			m_letter_mesh.RecalculateNormals();
			if(!m_letter_mesh.normals[0].Equals(Vector3.zero))
			{
				Quaternion rotation = Quaternion.LookRotation(m_letter_mesh.normals[0], m_letter_mesh.vertices[1].Equals(m_letter_mesh.vertices[2]) ? Vector3.forward : m_letter_mesh.vertices[1] - m_letter_mesh.vertices[2]);
				m_transform.position = m_effect_manager_handle.m_transform.position + (rotation * m_position_offset) + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4;
				m_transform.rotation = rotation;
			}
			else
			{
				m_transform.position = m_effect_manager_handle.m_transform.position + m_position_offset + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4;
			}
		}
		
		m_duration -= delta_time;
		
		if(m_duration > 0)
		{
			return false;
		}
		
		m_emitter.emit = false;
		
		if(m_emitter.particleCount > 0)
		{
			return false;
		}
		
		return true;
	}
	
	public void Stop(bool force_stop)
	{
		m_emitter.emit = false;
		
		if(force_stop)
		{
			m_emitter.ClearParticles();
		}
	}
}