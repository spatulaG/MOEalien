using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(EffectManager))]
public class EffectManager_Inspector : Editor
{
	bool m_previewing_anim = false;			// Denotes whether the animation is currently being previewed in the editor
	bool m_paused = false;
	EffectManager font_manager;
	float m_old_time = 0;
	
	float m_set_text_delay = 0;
	float m_time_delta = 0;
	
	string m_old_text;
	TextDisplayAxis m_old_display_axis;
	TextAnchor m_old_text_anchor;
	TextAlignment m_old_text_alignment;
	float m_old_char_size;
	float m_old_line_height;
	Vector2 m_old_px_offset;
	float m_old_max_width;
	
    void OnEnable()
    {
		EditorApplication.update += UpdateFunction;
    }
	
	void OnDisable()
	{
		EditorApplication.update -= UpdateFunction;
	}
	
	void UpdateFunction()
	{
		m_time_delta = Time.realtimeSinceStartup - m_old_time;
		
		if(m_previewing_anim && !m_paused)
		{
			if(font_manager == null)
			{
				font_manager = (EffectManager)target;
			}
			
			if(m_time_delta > 0 && !font_manager.UpdateAnimation(m_time_delta))
			{
				m_previewing_anim = false;
			}
		}
		
		if(m_set_text_delay > 0)
		{
			m_set_text_delay -= m_time_delta;
			
			if(m_set_text_delay <= 0)
			{
				m_set_text_delay = 0;
				
				font_manager.SetText(font_manager.m_text);
			}
		}
		
		m_old_time = Time.realtimeSinceStartup;
	}
	
	public override void OnInspectorGUI ()
	{
//		DrawDefaultInspector();
		
		font_manager = (EffectManager)target;
		
		m_old_text = font_manager.m_text;
		m_old_display_axis = font_manager.m_display_axis;
		m_old_text_anchor = font_manager.m_text_anchor;
		m_old_text_alignment = font_manager.m_text_alignment;
		m_old_char_size = font_manager.m_character_size;
		m_old_line_height = font_manager.m_line_height;
		m_old_px_offset = font_manager.m_px_offset;
		m_old_max_width = font_manager.m_max_width;
		
		if(GUI.changed)
		{
			return;
		}
		
		EditorGUILayout.LabelField("Font Setup Data", EditorStyles.boldLabel);
		
#if !UNITY_3_5
		font_manager.m_font = EditorGUILayout.ObjectField(new GUIContent("Font (.ttf, .dfont, .otf)", "Your font file to use for this text."), font_manager.m_font, typeof(Font), true) as Font;
		if(GUI.changed && font_manager.m_font != null)
		{
			font_manager.gameObject.GetComponent<Renderer>().material = font_manager.m_font.material;
			font_manager.m_font_material = font_manager.m_font.material;
			font_manager.SetText(font_manager.m_text, true);
		}
#endif
		
		font_manager.m_font_data_file = EditorGUILayout.ObjectField(new GUIContent("Font Data File", "Your Bitmap font text data file."), font_manager.m_font_data_file, typeof(TextAsset), true) as TextAsset;
		if(GUI.changed && font_manager.m_font_data_file != null && font_manager.m_font_material != null)
		{
			// Wipe the old character data hashtable
			font_manager.ClearFontCharacterData();
			font_manager.SetText(font_manager.m_text, true);
			return;
		}
		font_manager.m_font_material = EditorGUILayout.ObjectField(new GUIContent("Font Material", "Your Bitmap font material"), font_manager.m_font_material, typeof(Material), true) as Material;
		if(GUI.changed && font_manager.m_font_data_file != null && font_manager.m_font_material != null)
		{
			// Reset the text with the new material assigned.
			font_manager.gameObject.GetComponent<Renderer>().material = font_manager.m_font_material;
			font_manager.SetText(font_manager.m_text, true);
			return;
		}
		EditorGUILayout.Separator();
		
		EditorGUILayout.LabelField(new GUIContent("Text", "The text to display."), EditorStyles.boldLabel);
		font_manager.m_text = EditorGUILayout.TextArea(font_manager.m_text, GUILayout.Width(Screen.width - 25));
		EditorGUILayout.Separator();
		
		EditorGUILayout.LabelField("Text Settings", EditorStyles.boldLabel);
		font_manager.m_display_axis = (TextDisplayAxis) EditorGUILayout.EnumPopup(new GUIContent("Display Axis", "Denotes whether to render the text horizontally or vertically."), font_manager.m_display_axis);
		font_manager.m_text_anchor = (TextAnchor) EditorGUILayout.EnumPopup(new GUIContent("Text Anchor", "Defines the anchor point about which the text is rendered"), font_manager.m_text_anchor);
		font_manager.m_text_alignment = (TextAlignment) EditorGUILayout.EnumPopup(new GUIContent("Text Alignment", "Defines the alignment of the text, just like your favourite word processor."), font_manager.m_text_alignment);
		font_manager.m_character_size = EditorGUILayout.FloatField(new GUIContent("Character Size", "Specifies the size of the text."), font_manager.m_character_size);
		font_manager.m_line_height = EditorGUILayout.FloatField(new GUIContent("Line Height", "Defines the height of the text lines, based on the tallest line. If value is 2, the lines will be spaced at double the height of the tallest line."), font_manager.m_line_height);
		font_manager.m_px_offset = EditorGUILayout.Vector2Field("Letter Spacing Offset", font_manager.m_px_offset);
		font_manager.m_max_width = EditorGUILayout.FloatField(new GUIContent("Max Width", "Defines the maximum width of the text, and breaks the text onto new lines to keep it within this maximum."), font_manager.m_max_width);
		EditorGUILayout.Separator();
		
		EditorGUILayout.LabelField("Effect Settings", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		font_manager.m_begin_on_start = EditorGUILayout.Toggle(new GUIContent("Play On Start", "Should this effect be automatically triggered when it's first started in the scene?"), font_manager.m_begin_on_start);
		if(font_manager.m_begin_on_start)
		{
			font_manager.m_begin_delay = EditorGUILayout.FloatField(new GUIContent("Delay", "How much the effect is delayed after first being started."), font_manager.m_begin_delay);
			if(font_manager.m_begin_delay < 0)
			{
				font_manager.m_begin_delay = 0;
			}
		}
		EditorGUILayout.EndHorizontal();
		font_manager.m_on_finish_action = (ON_FINISH_ACTION) EditorGUILayout.EnumPopup(new GUIContent("On Finish Action", "What should happen when the effect finishes?"), font_manager.m_on_finish_action);
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(!m_previewing_anim || m_paused ? "Play" : "Pause"))
		{
			if(m_previewing_anim)
			{
				m_paused = !m_paused;
			}
			else
			{
				m_previewing_anim = true;
				
				font_manager.PlayAnimation();
				m_paused = false;
			}
		
			m_old_time = Time.realtimeSinceStartup;
		}
		if(GUILayout.Button("Reset"))
		{
			m_paused = true;
			m_previewing_anim = false;
			font_manager.ResetAnimation();
		}
		EditorGUILayout.EndHorizontal();
		
		// Render continue animation buttons
		if(font_manager.Playing)
		{
			EditorGUILayout.BeginHorizontal();
			
			if(font_manager.m_master_animations.Count > 1)
			{
				int continue_count = 0;
				foreach(LetterAnimation animation in font_manager.m_master_animations)
				{
					if(animation.CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING)
					{
						if(GUILayout.Button("Continue[" + (continue_count+1) + "]"))
						{
							font_manager.ContinueAnimation(continue_count);
						}
					}
					continue_count ++;
				}
			}
			
			EditorGUILayout.EndHorizontal();
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(font_manager);
		}
		
		if(m_old_char_size != font_manager.m_character_size ||
			m_old_display_axis != font_manager.m_display_axis ||
			m_old_line_height != font_manager.m_line_height ||
			m_old_max_width != font_manager.m_max_width ||
			!m_old_text.Equals(font_manager.m_text)	||
			m_old_text_alignment != font_manager.m_text_alignment ||
			m_old_text_anchor != font_manager.m_text_anchor ||
			m_old_px_offset != font_manager.m_px_offset)
		{
			font_manager.SetText(font_manager.m_text);
		}
		
		
		if (GUILayout.Button("Open Animation Editor"))
		{
			EditorWindow.GetWindow(typeof(TextEffectsManager));
		}
	}
}