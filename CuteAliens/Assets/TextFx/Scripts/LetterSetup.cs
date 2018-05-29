using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimationStateVariables
{
	public bool m_active;
	public bool m_waiting_to_sync;
	public bool m_started_action;			// triggered when action starts (after initial delay)
	public float m_break_delay;
	public float m_timer_offset;
	public int m_action_index;
	public bool m_reverse;
	public int m_action_index_progress;		// Used to track progress through a loop cycle
	public int m_prev_action_index;
	public float m_linear_progress;
	public float m_action_progress;
	public List<ActionLoopCycle> m_active_loop_cycles;
	
	public AnimationStateVariables Clone()
	{
		return new AnimationStateVariables(){
			m_active = m_active,
			m_waiting_to_sync = m_waiting_to_sync,
			m_started_action = m_started_action,
			m_break_delay = m_break_delay,
			m_timer_offset = m_timer_offset,
			m_action_index = m_action_index,
			m_reverse = m_reverse,
			m_action_index_progress = m_action_index_progress,
			m_prev_action_index = m_prev_action_index,
			m_linear_progress = m_linear_progress,
			m_action_progress = m_action_progress,
			m_active_loop_cycles = m_active_loop_cycles
		};
	}
	
	public void Reset()
	{
		m_active = false;
		m_waiting_to_sync = false;
		m_started_action = false;
		m_break_delay = 0;
		m_timer_offset = 0;
		m_action_index = 0;
		m_reverse = false;
		m_action_index_progress = 0;
		m_prev_action_index = -1;
		m_linear_progress = 0;
		m_action_progress = 0;
		m_active_loop_cycles.Clear();
	}
}

[System.Serializable]
public class LetterSetup
{
	public string m_character;
	public bool m_flipped = false;
	public float m_offset_width = 0;
	public float m_width = 0;
	public float m_height = 0;
	public Vector3[] m_base_vertices;
	public Vector3 m_base_offset;
	public Mesh m_mesh;
	public float m_x_offset = 0;
	public float m_y_offset = 0;
	public bool m_base_offsets_setup = false;
	public AnimationProgressionVariables m_progression_variables;
	
	[SerializeField]
	AnimationStateVariables m_anim_state_vars;
	
	LetterAction m_current_letter_action = null;
	float m_action_timer;
	float m_action_delay;
	float m_action_duration;
	int finished_action = -1;
	VertexColour start_colour, end_colour;			// Used to store the colours for each frame
	AnimatePerOptions m_last_animate_per;
	
	// Getter / Setters
	public AnimationStateVariables AnimStateVars { get { return m_anim_state_vars; } }
	public List<ActionLoopCycle> ActiveLoopCycles { get { return m_anim_state_vars.m_active_loop_cycles; } }
	public bool WaitingToSync { get { return m_anim_state_vars.m_waiting_to_sync; } }
	public int ActionIndex { get { return m_anim_state_vars.m_action_index; } }
	public bool InReverse { get { return m_anim_state_vars.m_reverse; } }
	public int ActionProgress { get { return m_anim_state_vars.m_action_index_progress; } }
	public bool Active { get { return m_anim_state_vars.m_active; } set { m_anim_state_vars.m_active = value; } }
	
	
	public LetterSetup(string character, int letter_idx, Mesh mesh, Vector3 base_offset, ref CustomCharacterInfo char_info, int line_num, int word_idx)
	{
		m_character = character;
		m_mesh = mesh;
		m_base_offset = base_offset;
		
		m_progression_variables = new AnimationProgressionVariables(letter_idx, word_idx, line_num);
		
		m_anim_state_vars = new AnimationStateVariables();
		m_anim_state_vars.m_active_loop_cycles = new List<ActionLoopCycle>();
		
		SetupLetterMesh(ref char_info);
		
		if(m_flipped)
		{
			// flip UV coords in x axis.
			m_mesh.uv = new Vector2[] {mesh.uv[3], mesh.uv[2], mesh.uv[1], mesh.uv[0]};
		}
	}
	
	public void Recycle(string character, int letter_idx, Mesh mesh, Vector3 base_offset, ref CustomCharacterInfo char_info, int line_num, int word_idx)
	{
		m_character = character;
		m_mesh = mesh;
		m_base_offset = base_offset;
		
		m_progression_variables = new AnimationProgressionVariables(letter_idx, word_idx, line_num);
		
		SetupLetterMesh(ref char_info);
		
		if(m_flipped)
		{
			// flip UV coords in x axis.
			m_mesh.uv = new Vector2[] {mesh.uv[3], mesh.uv[2], mesh.uv[1], mesh.uv[0]};
		}
		
		m_current_letter_action = null;
	}
	
	public void SetupLetterMesh(ref CustomCharacterInfo char_info)
	{
		m_offset_width = char_info.width;
		m_width = char_info.vert.width;
		m_height = char_info.vert.height;
		m_flipped = char_info.flipped;
		
		// Setup base vertices
		m_x_offset = char_info.vert.x;
		m_y_offset = char_info.vert.y;
		
		if(!m_flipped)
		{
			m_base_vertices = new Vector3[] { new Vector3(m_width, 0, 0), new Vector3(0, 0, 0), new Vector3(0, m_height, 0), new Vector3(m_width, m_height, 0)};
		}
		else
		{
			// rotate order of vertices by one.
			m_base_vertices = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, m_height, 0), new Vector3( m_width, m_height, 0), new Vector3(m_width, 0, 0)};
		}
	}
	
	public void SetAnimationVars(LetterSetup master_letter)
	{
		m_anim_state_vars = master_letter.AnimStateVars.Clone();
		
		m_current_letter_action = null;
		
		// clone the list of active loop cycles, so that the list reference is not shared between two letters
		m_anim_state_vars.m_active_loop_cycles = new List<ActionLoopCycle>();
		foreach(ActionLoopCycle loop_cycle in master_letter.AnimStateVars.m_active_loop_cycles)
		{
			m_anim_state_vars.m_active_loop_cycles.Add(loop_cycle.Clone());
		}
	}
	
	public void Reset(LetterAnimation animation)
	{
		m_anim_state_vars.Reset();
		
		if(animation.m_loop_cycles.Count > 0)
		{
			UpdateLoopList(animation);
		}
	}
	
	public void SetBaseOffset(TextAnchor anchor, TextDisplayAxis display_axis, TextAlignment alignment, List<TextSizeData> text_datas)
	{
		SetupBaseOffsets(anchor, display_axis, alignment, text_datas[m_progression_variables.m_line_value]);
		
		m_base_offsets_setup = true;
	}
	
	void SetupBaseOffsets(TextAnchor anchor, TextDisplayAxis display_axis, TextAlignment alignment, TextSizeData text_data)
	{
		if(display_axis == TextDisplayAxis.HORIZONTAL)
		{
			m_base_offset += new Vector3(m_x_offset, m_y_offset - text_data.m_line_height_offset, 0);
		}
		else
		{
			m_base_offset += new Vector3(text_data.m_line_height_offset, 0, 0);
		}
		
		m_base_offset.y -= text_data.m_y_max;
		
		// Handle text y offset
		if(anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight)
		{
			m_base_offset.y += text_data.m_total_text_height / 2;
		}
		else if(anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight)
		{
			m_base_offset.y += text_data.m_total_text_height;
		}
		
		float alignment_offset = 0;
		if(display_axis == TextDisplayAxis.HORIZONTAL)
		{
			if(alignment == TextAlignment.Center)
			{
				alignment_offset = (text_data.m_total_text_width - text_data.m_text_line_width) / 2;
			}
			else if(alignment == TextAlignment.Right)
			{
				alignment_offset = (text_data.m_total_text_width - text_data.m_text_line_width);
			}
		}
		else
		{
			if(alignment == TextAlignment.Center)
			{
				m_base_offset.y -= (text_data.m_total_text_height - text_data.m_text_line_height) / 2;
			}
			else if(alignment == TextAlignment.Right)
			{
				m_base_offset.y -= (text_data.m_total_text_height - text_data.m_text_line_height);
			}
		}
		
		// Handle text x offset
		if(anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight)
		{
			m_base_offset.x -= text_data.m_total_text_width - alignment_offset;
		}
		else if(anchor == TextAnchor.LowerCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter)
		{
			m_base_offset.x -= (text_data.m_total_text_width/2) - alignment_offset;
		}
		else
		{
			m_base_offset.x += alignment_offset;
		}
	}
	
	public void SetMeshState(int action_idx, float action_progress, LetterAnimation animation, AnimatePerOptions animate_per)
	{
		if(action_idx >= 0 && action_idx < animation.m_letter_actions.Count)
		{
			SetupMesh(animation.m_letter_actions[action_idx], Mathf.Clamp(action_progress, 0,1), true, m_progression_variables, animate_per, Mathf.Clamp(action_progress, 0,1));
		}
		else
		{
			// action not found for this letter. Position letter in its default position
			
			Vector3[] mesh_verts = new Vector3[4];
			for(int idx=0; idx < 4; idx++)
			{
				mesh_verts[idx] = m_base_vertices[idx] + m_base_offset;
			}
			m_mesh.vertices = mesh_verts;
			m_mesh.colors = new Color[]{Color.white, Color.white, Color.white, Color.white};
		}
	}
	
	void SetNextActionIndex(LetterAnimation animation)
	{
		// based on current active loop list, return the next action index
		
		// increment action progress count
		m_anim_state_vars.m_action_index_progress++;
		
		ActionLoopCycle current_loop;
		for(int loop_idx=0; loop_idx < m_anim_state_vars.m_active_loop_cycles.Count; loop_idx++)
		{
			current_loop = m_anim_state_vars.m_active_loop_cycles[loop_idx];
			
			if((current_loop.m_loop_type == LOOP_TYPE.LOOP && m_anim_state_vars.m_action_index == current_loop.m_end_action_idx) ||
				(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE && ((m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index == current_loop.m_start_action_idx) || (!m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index == current_loop.m_end_action_idx)))
			)
			{
				
				// Reached end of loop cycle. Deduct one cycle from loop count.
				bool end_of_loop_cycle = current_loop.m_loop_type == LOOP_TYPE.LOOP || m_anim_state_vars.m_reverse;
				
				if(end_of_loop_cycle)
				{
					current_loop.m_number_of_loops--;
				}
				
				// Switch reverse status
				if(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
				{
					m_anim_state_vars.m_reverse = !m_anim_state_vars.m_reverse;
				}
				
				current_loop.FirstPass = false;
				
				if(end_of_loop_cycle && current_loop.m_number_of_loops == 0)
				{
					// loop cycle finished
					// Remove this loop from active loop list
					m_anim_state_vars.m_active_loop_cycles.RemoveAt(loop_idx);
					loop_idx--;
					
					if(current_loop.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
					{
						// Don't allow anim to progress back through actions, skip to action beyond end of reverse loop
						m_anim_state_vars.m_action_index = current_loop.m_end_action_idx;
					}
				}
				else
				{
					if(current_loop.m_number_of_loops < 0)
					{
						current_loop.m_number_of_loops = -1;
					}
					
					// return to the start of this loop again
					if(current_loop.m_loop_type == LOOP_TYPE.LOOP)
					{
						m_anim_state_vars.m_action_index = current_loop.m_start_action_idx;
					}
					
					return;
				}
			}
			else
			{
				break;
			}
		}
		
		m_anim_state_vars.m_action_index += (m_anim_state_vars.m_reverse ? -1 : 1);
		
		// check for animation reaching end
		if(m_anim_state_vars.m_action_index >= animation.m_letter_actions.Count)
		{
			m_anim_state_vars.m_active = false;
			m_anim_state_vars.m_action_index = animation.m_letter_actions.Count -1;
		}
		
		return;
	}
	
	// Only called if action_idx has changed since last time
	void UpdateLoopList(LetterAnimation animation)
	{
		// add any new loops from the next action index to the loop list
		foreach(ActionLoopCycle loop in animation.m_loop_cycles)
		{
			if(loop.m_start_action_idx == m_anim_state_vars.m_action_index)
			{
				// add this new loop into the ordered active loop list
				int new_loop_cycle_span = loop.SpanWidth;
				
				int loop_idx = 0;
				foreach(ActionLoopCycle active_loop in m_anim_state_vars.m_active_loop_cycles)
				{
					if(loop.m_start_action_idx == active_loop.m_start_action_idx && loop.m_end_action_idx == active_loop.m_end_action_idx)
					{
						// This loop is already in the active loop list, don't re-add
						loop_idx = -1;
						break;
					}
					
					if(new_loop_cycle_span < active_loop.SpanWidth)
					{
						break;
					}
						
					loop_idx++;
				}
				
				if(loop_idx >= 0)
				{
					m_anim_state_vars.m_active_loop_cycles.Insert(loop_idx, loop.Clone());
				}
			}
		}
	}
	
	public void ContinueAction(float animation_timer, LetterAnimation animation, AnimatePerOptions animate_per)
	{
		if(m_anim_state_vars.m_waiting_to_sync)
		{
			m_anim_state_vars.m_break_delay = 0;
			m_anim_state_vars.m_waiting_to_sync= false;
			
			// reset timer offset to compensate for the sync-up wait time
			m_anim_state_vars.m_timer_offset = animation_timer;
			
			// Progress letter animation index to next, and break out of the loop
			int prev_action_idx = m_anim_state_vars.m_action_index;
			
			// Set next action index
			SetNextActionIndex(animation);
			
			if(m_anim_state_vars.m_active)
			{
				if(!m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index_progress > m_anim_state_vars.m_action_index)
				{
					// Repeating the action again; check for unqiue random variable requests.
					animation.m_letter_actions[m_anim_state_vars.m_action_index].SoftReset(animation.m_letter_actions[prev_action_idx], m_progression_variables, animate_per);
				}
				
				if(prev_action_idx != m_anim_state_vars.m_action_index)
				{
					UpdateLoopList(animation);
				}
			}
		}		
	}
	
	void SetCurrentLetterAction(LetterAction letter_action)
	{
		m_current_letter_action = letter_action;
		
		m_action_delay = Mathf.Max(m_current_letter_action.m_delay_progression.GetValue(m_progression_variables, m_last_animate_per), 0);
		m_action_duration = Mathf.Max(m_current_letter_action.m_duration_progression.GetValue(m_progression_variables, m_last_animate_per), 0);
		
		// Check if action is in a loopreverse_onetime delay case. If so, set delay to 0.
		if(	m_anim_state_vars.m_active_loop_cycles != null &&
			m_anim_state_vars.m_active_loop_cycles.Count > 0 &&
			m_anim_state_vars.m_active_loop_cycles[0].m_delay_first_only &&
			!m_anim_state_vars.m_active_loop_cycles[0].FirstPass &&
			m_current_letter_action.m_delay_progression.m_progression != ValueProgression.Constant)
		{
			if(m_anim_state_vars.m_reverse || !m_current_letter_action.m_force_same_start_time)
			{
				m_action_delay = 0;
			}
		}
	}
	
	// Animates the letter mesh and return the current action index in use
	public LETTER_ANIMATION_STATE AnimateMesh(	bool force_render,
												float timer,
												TextAnchor text_anchor,
												int lowest_action_progress,
												LetterAnimation animation,
												AnimatePerOptions animate_per,
												float delta_time,
												EffectManager effect_manager)
	{
		m_last_animate_per = animate_per;
		
		if(animation.m_letter_actions.Count > 0 && m_anim_state_vars.m_action_index < animation.m_letter_actions.Count)
		{
			if(!m_anim_state_vars.m_active && !force_render)
			{
				return LETTER_ANIMATION_STATE.STOPPED;
			}
			
			bool first_action_call = false;
			
			if(m_anim_state_vars.m_action_index != m_anim_state_vars.m_prev_action_index)
			{
				SetCurrentLetterAction(animation.m_letter_actions[m_anim_state_vars.m_action_index]);
				first_action_call = true;
				
				m_anim_state_vars.m_started_action = false;
			}
			else if(m_current_letter_action == null)
			{
				SetCurrentLetterAction(animation.m_letter_actions[m_anim_state_vars.m_action_index]);
			}
			
			m_anim_state_vars.m_prev_action_index = m_anim_state_vars.m_action_index;
			
			if(force_render)
			{
				SetupMesh(m_current_letter_action, m_anim_state_vars.m_action_progress, true, m_progression_variables, animate_per, m_anim_state_vars.m_linear_progress);
			}
			
			if(m_anim_state_vars.m_waiting_to_sync)
			{
				if(m_current_letter_action.m_action_type == ACTION_TYPE.BREAK)
				{
					if(!force_render && m_anim_state_vars.m_break_delay > 0)
					{
						m_anim_state_vars.m_break_delay -= delta_time;
						
						if(m_anim_state_vars.m_break_delay <= 0)
						{
							ContinueAction(timer, animation, animate_per);
							
							return LETTER_ANIMATION_STATE.PLAYING;
						}
					}
					
					return LETTER_ANIMATION_STATE.WAITING;
				}
				else if(lowest_action_progress < m_anim_state_vars.m_action_index_progress)
				{
					return LETTER_ANIMATION_STATE.PLAYING;
				}
				else if(!force_render)
				{
					m_anim_state_vars.m_waiting_to_sync = false;
					
					// reset timer offset to compensate for the sync-up wait time
					m_anim_state_vars.m_timer_offset = timer;
				}
			}
			else if(!force_render && (m_current_letter_action.m_action_type == ACTION_TYPE.BREAK || (!m_anim_state_vars.m_reverse && m_current_letter_action.m_force_same_start_time && lowest_action_progress < m_anim_state_vars.m_action_index_progress)))
			{
				// Force letter to wait for rest of letters to be in sync
				m_anim_state_vars.m_waiting_to_sync = true;
				
				m_anim_state_vars.m_break_delay = Mathf.Max(m_current_letter_action.m_duration_progression.GetValue(m_progression_variables, animate_per), 0);
				
				return LETTER_ANIMATION_STATE.PLAYING;
			}
			
			
			if(force_render)
			{
				return m_anim_state_vars.m_active ? LETTER_ANIMATION_STATE.PLAYING : LETTER_ANIMATION_STATE.STOPPED;
			}
			
			finished_action = -1;
			m_anim_state_vars.m_action_progress = 0;
			m_anim_state_vars.m_linear_progress = 0;
			
			m_action_timer = timer - m_anim_state_vars.m_timer_offset;
			
			if((m_anim_state_vars.m_reverse || m_action_timer > m_action_delay))
			{
				m_anim_state_vars.m_linear_progress = (m_action_timer - (m_anim_state_vars.m_reverse ? 0 : m_action_delay)) / m_action_duration;
				
				if(m_anim_state_vars.m_reverse)
				{
					if(m_action_timer >= m_action_duration)
					{
						m_anim_state_vars.m_linear_progress = 0;
					}
					else
					{
						m_anim_state_vars.m_linear_progress = 1 - m_anim_state_vars.m_linear_progress;
					}
				}
				
				
				if(!m_anim_state_vars.m_started_action)
				{
					// TODO: implement more robust check for "in-sync" actions having already played the asigned audio/effect
					
					// Just started animating action after delay. Handle any OnStart events
					if(m_current_letter_action.m_audio_on_start != null && (m_progression_variables.m_letter_value == 0 || !m_current_letter_action.m_starting_in_sync || m_current_letter_action.m_audio_on_start_delay.m_progression != ValueProgression.Constant))
					{
						effect_manager.PlayAudioClip(	m_current_letter_action.m_audio_on_start,
														m_current_letter_action.m_audio_on_start_delay.GetValue(m_progression_variables, animate_per),
														m_current_letter_action.m_audio_on_start_offset.GetValue(m_progression_variables, animate_per),
														m_current_letter_action.m_audio_on_start_volume.GetValue(m_progression_variables, animate_per),
														m_current_letter_action.m_audio_on_start_pitch.GetValue(m_progression_variables, animate_per));
					}
					
					if(m_current_letter_action.m_emitter_on_start != null && (m_current_letter_action.m_emitter_on_start_per_letter || m_progression_variables.m_letter_value == 0))
					{
						effect_manager.PlayParticleEffect(	m_current_letter_action.m_emitter_on_start,
															m_current_letter_action.m_emitter_on_start_delay.GetValue(m_progression_variables, animate_per),
															m_current_letter_action.m_emitter_on_start_duration.GetValue(m_progression_variables, animate_per),
															m_mesh,
															m_current_letter_action.m_emitter_on_start_offset.GetValue(m_progression_variables, animate_per),
															m_current_letter_action.m_emitter_on_start_follow_mesh);
					}
					
					m_anim_state_vars.m_started_action = true;
				}
				
				
				m_anim_state_vars.m_action_progress = EasingManager.GetEaseProgress(m_current_letter_action.m_ease_type, m_anim_state_vars.m_linear_progress);
				
				if((!m_anim_state_vars.m_reverse && m_anim_state_vars.m_linear_progress >= 1) || (m_anim_state_vars.m_reverse && m_action_timer >= m_action_duration + m_action_delay))
				{
					m_anim_state_vars.m_action_progress = m_anim_state_vars.m_reverse ? 0 : 1;
					m_anim_state_vars.m_linear_progress = m_anim_state_vars.m_reverse ? 0 : 1;
					
					if(!m_anim_state_vars.m_reverse)
					{
						finished_action = m_anim_state_vars.m_action_index;
					}
					
					int prev_action_idx = m_anim_state_vars.m_action_index;
					float prev_delay = m_action_delay;
					
					// Set next action index
					SetNextActionIndex(animation);
					
					if(m_anim_state_vars.m_active)
					{
						if(!m_anim_state_vars.m_reverse)
						{
							m_anim_state_vars.m_started_action = false;
						}
						
						if(!m_anim_state_vars.m_reverse && m_anim_state_vars.m_action_index_progress > m_anim_state_vars.m_action_index)
						{
							// Repeating the action again; check for unqiue random variable requests.
							animation.m_letter_actions[m_anim_state_vars.m_action_index].SoftReset(animation.m_letter_actions[prev_action_idx], m_progression_variables, animate_per, m_anim_state_vars.m_action_index == 0);
						}
						else if(m_anim_state_vars.m_reverse)
						{
							animation.m_letter_actions[m_anim_state_vars.m_action_index].SoftResetStarts(animation.m_letter_actions[prev_action_idx], m_progression_variables, animate_per);
						}
						
						// Add to the timer offset
						m_anim_state_vars.m_timer_offset += prev_delay + m_action_duration;
						
						if(prev_action_idx != m_anim_state_vars.m_action_index)
						{
							UpdateLoopList(animation);
						}
						else
						{
							SetCurrentLetterAction(animation.m_letter_actions[m_anim_state_vars.m_action_index]);
						}
					}
				}
			}
			
			SetupMesh(m_current_letter_action, m_anim_state_vars.m_action_progress, force_render || first_action_call, m_progression_variables, animate_per, m_anim_state_vars.m_linear_progress);
			
			if(finished_action > -1)
			{
				// TODO: implement more robust check for "in-sync" actions having already played the asigned audio/effect
				
				if(m_current_letter_action.m_audio_on_finish != null && (m_progression_variables.m_letter_value == 0 || !m_current_letter_action.m_starting_in_sync))
				{
					effect_manager.PlayAudioClip(	m_current_letter_action.m_audio_on_finish,
													m_current_letter_action.m_audio_on_finish_delay.GetValue(m_progression_variables, animate_per),
													m_current_letter_action.m_audio_on_finish_offset.GetValue(m_progression_variables, animate_per),
													m_current_letter_action.m_audio_on_finish_volume.GetValue(m_progression_variables, animate_per),
													m_current_letter_action.m_audio_on_finish_pitch.GetValue(m_progression_variables, animate_per));
				}
				
				if(m_current_letter_action.m_emitter_on_finish != null && (m_current_letter_action.m_emitter_on_finish_per_letter || m_progression_variables.m_letter_value == 0))					
				{
					effect_manager.PlayParticleEffect(	m_current_letter_action.m_emitter_on_finish,
														m_current_letter_action.m_emitter_on_finish_delay.GetValue(m_progression_variables, animate_per),
														m_current_letter_action.m_emitter_on_finish_duration.GetValue(m_progression_variables, animate_per),
														m_mesh,
														m_current_letter_action.m_emitter_on_finish_offset.GetValue(m_progression_variables, animate_per),
														m_current_letter_action.m_emitter_on_finish_follow_mesh);
				}
			}
		}
		else
		{
			// no actions found for this letter. Position letter in its default position
			Vector3[] mesh_verts = new Vector3[4];
			for(int idx=0; idx < 4; idx++)
			{
				mesh_verts[idx] = m_base_vertices[idx] + m_base_offset;
			}
			m_mesh.vertices = mesh_verts;
			
			m_anim_state_vars.m_active = false;
		}
		
		return m_anim_state_vars.m_active ? LETTER_ANIMATION_STATE.PLAYING : LETTER_ANIMATION_STATE.STOPPED;
	}
	
	void SetupMesh(LetterAction letter_action, float action_progress, bool first_action_call, AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, float linear_progress)
	{	
		if(first_action_call || !letter_action.StaticPosition || !letter_action.StaticRotation || !letter_action.StaticScale)
		{
			Vector3[] mesh_verts = new Vector3[4];
			for(int idx=0; idx < 4; idx++)
			{
				// rotate vertices
				// handle letter anchor x-offset
				Vector3 anchor_offset = Vector3.zero;
				if(letter_action.m_letter_anchor == TextAnchor.UpperRight || letter_action.m_letter_anchor == TextAnchor.MiddleRight || letter_action.m_letter_anchor == TextAnchor.LowerRight)
				{
					anchor_offset += new Vector3(m_width, 0, 0);
				}
				else if(letter_action.m_letter_anchor == TextAnchor.UpperCenter || letter_action.m_letter_anchor == TextAnchor.MiddleCenter || letter_action.m_letter_anchor == TextAnchor.LowerCenter)
				{
					anchor_offset += new Vector3(m_width / 2, 0, 0);
				}
				
				// handle letter anchor y-offset
				if(letter_action.m_letter_anchor == TextAnchor.MiddleLeft || letter_action.m_letter_anchor == TextAnchor.MiddleCenter || letter_action.m_letter_anchor == TextAnchor.MiddleRight)
				{
					anchor_offset += new Vector3(0, m_height / 2, 0);
				}
				else if(letter_action.m_letter_anchor == TextAnchor.LowerLeft || letter_action.m_letter_anchor == TextAnchor.LowerCenter || letter_action.m_letter_anchor == TextAnchor.LowerRight)
				{
					anchor_offset += new Vector3(0, m_height, 0);
				}
				
				mesh_verts[idx] = m_base_vertices[idx];
				
				mesh_verts[idx] -= anchor_offset;
				
				// Scale verts
//				if(first_action_call) // || !letter_action.StaticScale)
//				{
					Vector3 from_scale = letter_action.m_start_scale.GetValue(progression_variables, animate_per);
					Vector3 to_scale = letter_action.m_end_scale.GetValue(progression_variables, animate_per);
					
					if(letter_action.m_scale_axis_ease_data.m_override_default)
					{
						mesh_verts[idx] = Vector3.Scale(mesh_verts[idx],
										new Vector3(	EffectManager.FloatLerp(from_scale.x, to_scale.x, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_x_ease, linear_progress)),
														EffectManager.FloatLerp(from_scale.y, to_scale.y, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_y_ease, linear_progress)),
														EffectManager.FloatLerp(from_scale.z, to_scale.z, EasingManager.GetEaseProgress(letter_action.m_scale_axis_ease_data.m_z_ease, linear_progress))));
					}
					else
					{
						mesh_verts[idx] = Vector3.Scale(mesh_verts[idx],
							EffectManager.Vector3Lerp(
							from_scale,
							to_scale,
							action_progress)
							);
					}
//				}
				
				// Rotate vert
//				if(first_action_call) // || !letter_action.StaticRotation)
//				{
					Vector3 from_rotation = letter_action.m_start_euler_rotation.GetValue(progression_variables, animate_per);
					Vector3 to_rotation = letter_action.m_end_euler_rotation.GetValue(progression_variables, animate_per);
					
					if(letter_action.m_rotation_axis_ease_data.m_override_default)
					{
						mesh_verts[idx] = 	Quaternion.Euler
											(
												EffectManager.FloatLerp(from_rotation.x, to_rotation.x, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_x_ease, linear_progress)),
												EffectManager.FloatLerp(from_rotation.y, to_rotation.y, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_y_ease, linear_progress)),
												EffectManager.FloatLerp(from_rotation.z, to_rotation.z, EasingManager.GetEaseProgress(letter_action.m_rotation_axis_ease_data.m_z_ease, linear_progress))
											) * mesh_verts[idx];;
					}
					else
					{
						mesh_verts[idx] = Quaternion.Euler(
													EffectManager.Vector3Lerp(
														from_rotation,
														to_rotation,
														action_progress)
													)
													* mesh_verts[idx];
					}
//				}
				
				mesh_verts[idx] += anchor_offset;
				
				// translate vert
				Vector3 from_pos = (!letter_action.m_start_pos.m_force_position_override ? m_base_offset : Vector3.zero) + letter_action.m_start_pos.GetValue(progression_variables, animate_per);
				Vector3 to_pos = (!letter_action.m_end_pos.m_force_position_override ? m_base_offset : Vector3.zero) + letter_action.m_end_pos.GetValue(progression_variables, animate_per);
				
				if(letter_action.m_position_axis_ease_data.m_override_default)
				{
					mesh_verts[idx] += new Vector3(	EffectManager.FloatLerp(from_pos.x, to_pos.x, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_x_ease, linear_progress)),
													EffectManager.FloatLerp(from_pos.y, to_pos.y, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_y_ease, linear_progress)),
													EffectManager.FloatLerp(from_pos.z, to_pos.z, EasingManager.GetEaseProgress(letter_action.m_position_axis_ease_data.m_z_ease, linear_progress)));
				}
				else
				{
					mesh_verts[idx] += EffectManager.Vector3Lerp(
						from_pos, 
						to_pos,
						action_progress);
				}
				
				
			}
			m_mesh.vertices = mesh_verts;
		}
		
		
		if(first_action_call || !letter_action.StaticColour)
		{
			if(letter_action.m_use_gradient_start || letter_action.m_use_gradient)
			{
				start_colour = letter_action.m_start_vertex_colour.GetValue(progression_variables, animate_per);
			}
			else
			{
				start_colour = new VertexColour(letter_action.m_start_colour.GetValue(progression_variables, animate_per));
			}
			
			if(letter_action.m_use_gradient_end || letter_action.m_use_gradient)
			{
				end_colour = letter_action.m_end_vertex_colour.GetValue(progression_variables, animate_per);
			}
			else
			{
				end_colour = new VertexColour(letter_action.m_end_colour.GetValue(progression_variables, animate_per));
			}
			
			if(!m_flipped)
			{
				m_mesh.colors = new Color[]{ 
					Color.Lerp(start_colour.top_right, end_colour.top_right, action_progress), 
					Color.Lerp(start_colour.top_left, end_colour.top_left, action_progress), 
					Color.Lerp(start_colour.bottom_left, end_colour.bottom_left, action_progress), 
					Color.Lerp(start_colour.bottom_right, end_colour.bottom_right, action_progress)};
			}
			else
			{
				m_mesh.colors = new Color[]{
					Color.Lerp(start_colour.top_left, end_colour.top_left, action_progress),
					Color.Lerp(start_colour.bottom_left, end_colour.bottom_left, action_progress),
					Color.Lerp(start_colour.bottom_right, end_colour.bottom_right, action_progress),
					Color.Lerp(start_colour.top_right, end_colour.top_right, action_progress)
				};
			}
		}
	}
}