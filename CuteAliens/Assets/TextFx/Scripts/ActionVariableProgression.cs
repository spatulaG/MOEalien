/**
	TextFx Variable Progression Classes.
	Used in animation Actions to define either a constant value, or an ordered or random sequence of values within a given range.
**/

using UnityEngine;

public enum ValueProgression
{
	Constant,
	Random,
	Eased
}

[System.Serializable]
public class ActionVariableProgression
{
	public ValueProgression m_progression = ValueProgression.Constant;
	public EasingEquation m_ease_type = EasingEquation.Linear;
	public bool m_is_offset_from_last = false;
	public bool m_to_to_bool = false;
	public bool m_unique_randoms = false;
	public AnimatePerOptions m_animate_per;
	public bool m_override_animate_per_option = false;
	
	public bool UniqueRandom { get { return m_progression == ValueProgression.Random && m_unique_randoms; } }
	
	public int GetProgressionIndex(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default)
	{
		return progression_variables.GetValue(m_override_animate_per_option ? m_animate_per : animate_per_default);
	}
}

[System.Serializable]
public class ActionFloatProgression : ActionVariableProgression
{
	public float[] m_values;
	public float m_from = 0;
	public float m_to = 0;
	public float m_to_to = 0;
	
	public float GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default)
	{
		return GetValue(GetProgressionIndex(progression_variables,animate_per_default));
	}
	
	public float GetValue(int progression_idx)
	{
		int num_vals = m_values.Length;
		if(num_vals > 1 && progression_idx < num_vals)
		{
			return m_values[progression_idx];
		}
		else if(num_vals==1)
		{
			return m_values[0];
		}
		else
		{
			return 0;
		}
	}
	
	public int NumEditorLines
	{
		get
		{
			if(m_progression == ValueProgression.Constant)
			{
				return 2;
			}
			else if(m_progression == ValueProgression.Random || (m_progression == ValueProgression.Eased && !m_to_to_bool))
			{
				return 4;
			}
			else
			{
				return 5;
			}
		}
	}
	
	public ActionFloatProgression(float start_val)
	{
		m_from = start_val;
		m_to = start_val;
		m_to_to = start_val;
	}
	
	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per)
	{
		m_values[GetProgressionIndex(progression_variables, animate_per)] = m_from + (m_to - m_from) * UnityEngine.Random.value;
	}
	
	public void CalculateProgressions(int num_progressions)
	{
		
		// Initialise array of values.
		m_values = new float[m_progression == ValueProgression.Eased || m_progression == ValueProgression.Random
								? num_progressions
								: 1];
		
		if(m_progression == ValueProgression.Random) //  && (progression >= 0 || m_unique_randoms))
		{
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] = m_from + (m_to - m_from) * UnityEngine.Random.value;
			}
		}
		else if(m_progression == ValueProgression.Eased)
		{
			float progression;
			for(int idx=0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
				
				if(m_to_to_bool)
				{
					if(progression <= 0.5f)
					{
						m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression/0.5f);
					}
					else
					{
						progression -= 0.5f;
						m_values[idx] = m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression/0.5f);
					}
				}
				else
				{
					m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
				}
			}
		}
		else if(m_progression == ValueProgression.Constant)
		{
			m_values[0] = m_from;
		}
	}
	
	public ActionFloatProgression Clone()
	{
		ActionFloatProgression float_progression = new ActionFloatProgression(0);
		
		float_progression.m_progression = m_progression;
		float_progression.m_ease_type = m_ease_type;
		float_progression.m_from = m_from;
		float_progression.m_to = m_to;
		float_progression.m_to_to = m_to_to;
		float_progression.m_to_to_bool = m_to_to_bool;
		float_progression.m_unique_randoms = m_unique_randoms;
		float_progression.m_override_animate_per_option = m_override_animate_per_option;
		float_progression.m_animate_per = m_animate_per;
		
		return float_progression;
	}
}

[System.Serializable]
public class ActionPositionVector3Progression : ActionVector3Progression
{
	public bool m_force_position_override = false;
	
	public override int NumEditorLines
	{
		get
		{
			if(m_progression == ValueProgression.Constant)
			{
				return 4;
			}
			else if(m_progression == ValueProgression.Random)
			{
				return 7;
			}
			else
			{
				return m_to_to_bool ? 8 : 6;
			}
		}
	}
	
	public ActionPositionVector3Progression(Vector3 start_vec)
	{
		m_from = start_vec;
		m_to = start_vec;
		m_to_to = start_vec;
	}
	
	public ActionPositionVector3Progression CloneThis()
	{
		ActionPositionVector3Progression progression = new ActionPositionVector3Progression(Vector3.zero);
		
		progression.m_progression = m_progression;
		progression.m_ease_type = m_ease_type;
		progression.m_from = m_from;
		progression.m_to = m_to;
		progression.m_to_to = m_to_to;
		progression.m_to_to_bool = m_to_to_bool;
		progression.m_is_offset_from_last = m_is_offset_from_last;
		progression.m_unique_randoms = m_unique_randoms;
		progression.m_force_position_override = m_force_position_override;
		progression.m_override_animate_per_option = m_override_animate_per_option;
		progression.m_animate_per = m_animate_per;
		
		return progression;
	}
}

[System.Serializable]
public class ActionVector3Progression : ActionVariableProgression
{
	public Vector3[] m_values;
	public Vector3 m_from = Vector3.zero;
	public Vector3 m_to = Vector3.zero;
	public Vector3 m_to_to = Vector3.zero;
	
	public EasingEquation m_x_ease = EasingEquation.Linear;
	public EasingEquation m_y_ease = EasingEquation.Linear;
	public EasingEquation m_z_ease = EasingEquation.Linear;
	public bool m_override_per_axis_easing = false;
	
	public Vector3 GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default)
	{
		return GetValue(GetProgressionIndex(progression_variables,animate_per_default));
	}
	
	public Vector3 GetValue(int progression_idx)
	{
		int num_vals = m_values.Length;
		if(num_vals > 1 && progression_idx < num_vals)
		{
			return m_values[progression_idx];
		}
		else if(num_vals==1)
		{
			return m_values[0];
		}
		else
		{
			return Vector3.zero;
		}
	}
	
	public virtual int NumEditorLines
	{
		get
		{
			if(m_progression == ValueProgression.Constant)
			{
				return 3;
			}
			else if(m_progression == ValueProgression.Random || (m_progression == ValueProgression.Eased && !m_to_to_bool))
			{
				return 6;
			}
			else
			{
				return 8;
			}
		}
	}
	
	public ActionVector3Progression()
	{
		
	}
	
	public ActionVector3Progression(Vector3 start_vec)
	{
		m_from = start_vec;
		m_to = start_vec;
		m_to_to = start_vec;
	}
	
	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, Vector3[] offset_vec)
	{
		int progression_idx = GetProgressionIndex(progression_variables, animate_per);
		bool constant_offset = offset_vec != null && offset_vec.Length == 1;
			
		m_values[progression_idx] = m_is_offset_from_last ? offset_vec[constant_offset ? 0 : progression_idx] : Vector3.zero;
		m_values[progression_idx] += new Vector3(m_from.x + (m_to.x - m_from.x) * UnityEngine.Random.value, m_from.y + (m_to.y - m_from.y) * UnityEngine.Random.value, m_from.z + (m_to.z - m_from.z) * UnityEngine.Random.value);
	}
	
	public void CalculateProgressions(int num_progressions, Vector3[] offset_vecs)
	{
		
		// Initialise the array of values. Array of only one if all progressions share the same constant value.
		if(m_progression == ValueProgression.Eased || m_progression == ValueProgression.Random || (m_is_offset_from_last && offset_vecs.Length > 1))
		{
			bool constant_offset = offset_vecs != null && offset_vecs.Length == 1;
			m_values = new Vector3[num_progressions];
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] = m_is_offset_from_last ? offset_vecs[constant_offset ? 0 : idx] : Vector3.zero;
			}
		}
		else
		{
			m_values = new Vector3[1]{m_is_offset_from_last && offset_vecs.Length >= 1 ? offset_vecs[0] : Vector3.zero};
		}
		
		if(m_progression == ValueProgression.Random)
		{
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] += new Vector3(m_from.x + (m_to.x - m_from.x) * UnityEngine.Random.value, m_from.y + (m_to.y - m_from.y) * UnityEngine.Random.value, m_from.z + (m_to.z - m_from.z) * UnityEngine.Random.value);
			}
		}
		else if(m_progression == ValueProgression.Eased)
		{
			float progression;
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
				
				if(m_to_to_bool)
				{
					if(progression <= 0.5f)
					{
						m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression/0.5f);
					}
					else
					{
						progression -= 0.5f;
						m_values[idx] += m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression/0.5f);
					}
				}
				else
				{
					m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
				}
			}
			
		}
		else if(m_progression == ValueProgression.Constant)
		{
			for(int idx=0; idx < m_values.Length; idx++)
			{
				m_values[idx] += m_from;
			}
		}
	}
	
	public ActionVector3Progression Clone()
	{
		ActionVector3Progression vector3_progression = new ActionVector3Progression(Vector3.zero);
		
		vector3_progression.m_progression = m_progression;
		vector3_progression.m_ease_type = m_ease_type;
		vector3_progression.m_from = m_from;
		vector3_progression.m_to = m_to;
		vector3_progression.m_to_to = m_to_to;
		vector3_progression.m_to_to_bool = m_to_to_bool;
		vector3_progression.m_is_offset_from_last = m_is_offset_from_last;
		vector3_progression.m_unique_randoms = m_unique_randoms;
		vector3_progression.m_override_animate_per_option = m_override_animate_per_option;
		vector3_progression.m_animate_per = m_animate_per;
		
		return vector3_progression;
	}
}


[System.Serializable]
public class ActionColorProgression : ActionVariableProgression
{
	public Color[] m_values;
	public Color m_from = Color.white;
	public Color m_to = Color.white;
	public Color m_to_to = Color.white;
	
	public Color GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default)
	{
		return GetValue(GetProgressionIndex(progression_variables,animate_per_default));
	}
	
	public Color GetValue(int progression_idx)
	{
		int num_vals = m_values.Length;
		if(num_vals > 1 && progression_idx < num_vals)
		{
			return m_values[progression_idx];
		}
		else if(num_vals==1)
		{
			return m_values[0];
		}
		else
		{
			return Color.white;
		}
	}
	
	public int NumEditorLines
	{
		get
		{
			return m_progression == ValueProgression.Constant ? 2 : 3;
		}
	}
	
	public ActionColorProgression(Color start_colour)
	{
		m_from = start_colour;
		m_to = start_colour;
		m_to_to = start_colour;
	}
	
	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, Color[] offset_cols)
	{
		int progression_idx = GetProgressionIndex(progression_variables, animate_per);
		bool constant_offset = offset_cols != null && offset_cols.Length == 1;
			
		m_values[progression_idx] = m_is_offset_from_last ? offset_cols[constant_offset ? 0 : progression_idx] : new Color(0,0,0,0);
		m_values[progression_idx] += m_from + (m_to - m_from) * UnityEngine.Random.value;
	}
	
	public void CalculateProgressions(int num_progressions, Color[] offset_cols)
	{
		
		if(m_progression == ValueProgression.Eased || m_progression == ValueProgression.Random || (m_is_offset_from_last && offset_cols.Length > 1))
		{
			bool constant_offset = offset_cols != null && offset_cols.Length == 1;
			m_values = new Color[num_progressions];
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] = m_is_offset_from_last ? offset_cols[constant_offset ? 0 : idx] : new Color(0,0,0,0);
			}
		}
		else
		{
			m_values = new Color[1]{ m_is_offset_from_last ? offset_cols[0] : new Color(0,0,0,0) };
		}
		
		if(m_progression == ValueProgression.Random) // && (progression >= 0 || m_unique_randoms))
		{
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] += m_from + (m_to - m_from) * UnityEngine.Random.value;
			}
		}
		else if(m_progression == ValueProgression.Eased)
		{
			float progression;
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
				
				if(m_to_to_bool)
				{
					if(progression  <= 0.5f)
					{
						m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression/0.5f);
					}
					else
					{
						progression -= 0.5f;
						m_values[idx] += m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression/0.5f);
					}
				}
				else
				{
					m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
				}
			}
		}
		else if(m_progression == ValueProgression.Constant)
		{
			for(int idx=0; idx < m_values.Length; idx++)
			{
				m_values[idx] += m_from;
			}
		}
	}
	
	public ActionColorProgression Clone()
	{
		ActionColorProgression color_progression = new ActionColorProgression(Color.white);
		
		color_progression.m_progression = m_progression;
		color_progression.m_ease_type = m_ease_type;
		color_progression.m_from = m_from;
		color_progression.m_to = m_to;
		color_progression.m_to_to = m_to_to;
		color_progression.m_to_to_bool = m_to_to_bool;
		color_progression.m_is_offset_from_last = m_is_offset_from_last;
		color_progression.m_unique_randoms = m_unique_randoms;
		color_progression.m_override_animate_per_option = m_override_animate_per_option;
		color_progression.m_animate_per = m_animate_per;
		
		return color_progression;
	}
}

[System.Serializable]
public class ActionVertexColorProgression : ActionVariableProgression
{
	public VertexColour[] m_values;
	public VertexColour m_from = new VertexColour();
	public VertexColour m_to = new VertexColour();
	public VertexColour m_to_to = new VertexColour();
	
	public VertexColour GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default)
	{
		return GetValue(GetProgressionIndex(progression_variables,animate_per_default));
	}
	
	public VertexColour GetValue(int progression_idx)
	{
		int num_vals = m_values.Length;
		if(num_vals > 1 && progression_idx < num_vals)
		{
			return m_values[progression_idx];
		}
		else if(num_vals==1)
		{
			return m_values[0];
		}
		else
		{
			return new VertexColour(Color.white);
		}
	}
	
	public int NumEditorLines
	{
		get
		{
			return m_progression == ValueProgression.Constant ? 3 : 4;
		}
	}
	
	public ActionVertexColorProgression(VertexColour start_colour)
	{
		m_from = start_colour.Clone();
		m_to = start_colour.Clone();
		m_to_to = start_colour.Clone();
	}
	
	public void ConvertFromFlatColourProg(ActionColorProgression flat_colour_progression)
	{
		m_progression = flat_colour_progression.m_progression;
		m_ease_type = flat_colour_progression.m_ease_type;
		m_from = new VertexColour(flat_colour_progression.m_from);
		m_to = new VertexColour(flat_colour_progression.m_to);
		m_to_to = new VertexColour(flat_colour_progression.m_to_to);
		m_to_to_bool = flat_colour_progression.m_to_to_bool;
		m_is_offset_from_last = flat_colour_progression.m_is_offset_from_last;
		m_unique_randoms = flat_colour_progression.m_unique_randoms;
	}
	
	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, VertexColour[] offset_colours)
	{
		int progression_idx = GetProgressionIndex(progression_variables, animate_per);
		bool constant_offset = offset_colours != null && offset_colours.Length == 1;
			
		m_values[progression_idx] = m_is_offset_from_last ? offset_colours[constant_offset ? 0 : progression_idx].Clone() : new VertexColour(new Color(0,0,0,0));
		m_values[progression_idx] = m_values[progression_idx].Add(m_from.Add(m_to.Sub(m_from).Multiply(UnityEngine.Random.value)));
	}
	
	public void CalculateProgressions(int num_progressions, VertexColour[] offset_vert_colours, Color[] offset_colours)
	{
		if(m_progression == ValueProgression.Eased || m_progression == ValueProgression.Random || (m_is_offset_from_last && ((offset_colours != null && offset_colours.Length > 1) || (offset_vert_colours != null && offset_vert_colours.Length > 1) )))
		{
			bool constant_offset = (offset_colours != null && offset_colours.Length == 1) || (offset_vert_colours != null && offset_vert_colours.Length == 1);
			m_values = new VertexColour[num_progressions];
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] = m_is_offset_from_last ? 
									(offset_colours != null ? new VertexColour(offset_colours[constant_offset ? 0 : idx]) : offset_vert_colours[constant_offset ? 0 : idx].Clone())
						
									: new VertexColour(new Color(0,0,0,0));
			}
		}
		else
		{
			m_values = new VertexColour[1]{ m_is_offset_from_last ? 
									(offset_colours != null ? new VertexColour(offset_colours[0]) : offset_vert_colours[0].Clone())
						
									: new VertexColour(new Color(0,0,0,0)) };
		}
		
		
		if(m_progression == ValueProgression.Random)
		{
			for(int idx=0; idx < num_progressions; idx++)
			{
				m_values[idx] = m_values[idx].Add(m_from.Add(m_to.Sub(m_from).Multiply(UnityEngine.Random.value)));
			}
		}
		else if(m_progression == ValueProgression.Eased)
		{
			float progression;
			
			for(int idx=0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
			
				if(m_to_to_bool)
				{
					if(progression  <= 0.5f)
					{
						m_values[idx] = m_values[idx].Add(m_from.Add((m_to.Sub(m_from)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression/0.5f))));
					}
					else
					{
						progression -= 0.5f;
						m_values[idx] = m_values[idx].Add(m_to.Add((m_to_to.Sub(m_to)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression/0.5f))));
					}
				}
				else
				{
					m_values[idx] = m_values[idx].Add(m_from.Add((m_to.Sub(m_from)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression))));
				}
			}
		}
		else if(m_progression == ValueProgression.Constant)
		{
			for(int idx=0; idx < m_values.Length; idx++)
			{
				m_values[idx] = m_values[idx].Add(m_from);
			}
		}
	}
	
	public ActionVertexColorProgression Clone()
	{
		ActionVertexColorProgression color_progression = new ActionVertexColorProgression(new VertexColour());
		
		color_progression.m_progression = m_progression;
		color_progression.m_ease_type = m_ease_type;
		color_progression.m_from = m_from.Clone();
		color_progression.m_to = m_to.Clone();
		color_progression.m_to_to = m_to_to.Clone();
		color_progression.m_to_to_bool = m_to_to_bool;
		color_progression.m_is_offset_from_last = m_is_offset_from_last;
		color_progression.m_unique_randoms = m_unique_randoms;
		color_progression.m_override_animate_per_option = m_override_animate_per_option;
		color_progression.m_animate_per = m_animate_per;
		
		return color_progression;
	}
}