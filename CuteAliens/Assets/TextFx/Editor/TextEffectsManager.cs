using UnityEngine;
using UnityEditor;

public class TextEffectsManager : EditorWindow
{
	static TextEffectsManager m_instance;
	static TextEffectsManager Instance
	{
		get
		{
			if(m_instance == null)
			{
				m_instance = (TextEffectsManager)EditorWindow.GetWindow (typeof (TextEffectsManager));
			}
			
			return m_instance;
		}
	}
	
	// Gui layout variables
	const float WINDOW_BORDER_LEFT = 10;
	const float WINDOW_BORDER_RIGHT = 10;
	const float WINDOW_BORDER_TOP = 10;
	const float WINDOW_BORDER_BOTTOM = 10;
	const float TOOLBAR_LETTER_WIDTH = 40;
	const float TOOLBAR_LETTER_HEIGHT = 30;
	const float LINE_HEIGHT = 20;
	const float TEXT_AREA_LINE_HEIGHT = 14;
	const float HEADER_HEIGHT = 30;
	const float ENUM_SELECTOR_WIDTH = 300;
	const float ENUM_SELECTOR_WIDTH_MEDIUM = 120;
	const float ENUM_SELECTOR_WIDTH_SMALL = 70;
	const float ACTION_NODE_MARGIN = 5;
	
	const float PROGRESSION_HEADER_LABEL_WIDTH = 150;
	const float ACTION_INDENT_LEVEL_1 = 10;
	
	const float VECTOR_3_WIDTH = 300;
	
	const float ACTION_NODE_SPACING = 60;
	const int LOOP_LIST_WIDTH = 360; //325;
	const int BASE_LOOP_LIST_POSITION_OFFSET = 80;
	const int LOOP_LINE_OFFSET = 35;
	
	static Color LOOP_COLOUR = Color.blue;
	static Color LOOP_REVERSE_COLOUR = Color.red;
	
	static GUIStyle m_header_gui_style;
	static GUIStyle HeaderGuiStyle
	{
		get
		{
			if(m_header_gui_style == null)
			{
				m_header_gui_style = new GUIStyle(EditorStyles.foldout);
				m_header_gui_style.fontSize = 18;
				m_header_gui_style.padding.top = -2;
				m_header_gui_style.padding.left = 16;
			}
			
			return m_header_gui_style;
		}
	}
	
	// Gui Interaction variables
	Vector2 MAIN_SCROLL_POS = Vector2.zero;
	Vector2 LOOP_SCROLL_POS = Vector2.zero;
	Vector2 CUSTOM_LETTERS_LIST_POS = Vector2.zero;
	
	int ANIMATION_IDX = 0;
	bool m_display_loops_tree = false;
	
	Rect[] m_state_node_rects;
	const float state_overview_x = 5;
	int main_action_editor_x = 100;
	int state_overview_width = 0;
	int loop_tree_width = 60;
	
	int MainEditorX { get { return state_overview_width < main_action_editor_x ? main_action_editor_x : state_overview_width; } }
	
	int m_mouse_down_node_idx = -1;
	Vector2 m_mouse_down_pos;
	Vector2 m_mouse_drag_pos;
	
	EffectManager font_manager;
	bool m_previewing_anim = false;			// Denotes whether the animation is currently being previewed in the editor
	bool m_paused = false;
	float m_old_time = 0;
	
	bool ignore_gui_change = false;
	bool noticed_gui_change = false;
	int edited_action_idx = -1;
	bool editing_start_state = true;
	
	float m_time_delta = 0;
	
	// New Loop Vars
	int new_loop_from = 0;
	int new_loop_to = 0;
    
    [MenuItem ("Window/TextFX Manager")]
    static void Init ()
	{
        // Get existing open window or if none, make a new one:
        EditorWindow.GetWindow (typeof (TextEffectsManager));
    }
	
	void OnEnable()
    {
		EditorApplication.update += UpdateManager;
    }
	
	void OnDisable()
	{
		EditorApplication.update -= UpdateManager;
	}
	
	private void UpdateManager()
	{	
		if(m_previewing_anim && !m_paused)
		{
			if(font_manager == null)
			{
				m_previewing_anim = false;
				return;
			}
			
			m_time_delta = Time.realtimeSinceStartup - m_old_time;
			
			if(m_time_delta > 0 && m_time_delta < 0.5f && !font_manager.UpdateAnimation(m_time_delta))
			{
				m_previewing_anim = false;
			}
			
			m_old_time = Time.realtimeSinceStartup;
		}
	}
	
    
    void OnGUI ()
	{
		int edited_action = -1;
		bool start_of_action = true;
		DrawGUIWindow(out edited_action, out start_of_action);
		
		if (edited_action >= 0)
		{
			font_manager.SetAnimationState(edited_action, start_of_action ? 0 : 1, true);
			
			EditorUtility.SetDirty(font_manager);
		}
    }
	
	void OnInspectorUpdate()
	{
		Instance.Repaint();
	}
	
	
	void DrawGUIWindow(out int edited_action, out bool start_of_action)
	{
		// Set out parameters to a default value
		start_of_action = true;
		edited_action= -1;
		
		// Check whether a EffectManager object is selected
		if(Selection.gameObjects.Length == 1)
		{
			font_manager = Selection.gameObjects[0].GetComponent<EffectManager>();
		}
		
		if(font_manager == null)
		{
			return;
		}
		
		DrawEffectEditorPanel();
		
		if(!ignore_gui_change)
		{
			start_of_action = editing_start_state;
			edited_action = edited_action_idx;
		}
			
		return;
	}
	
	void DrawLoopTree(LetterAnimation selected_animation)
	{
		if(font_manager.m_master_animations.Count == 0)
		{
			return;
		}
		
		float main_editor_height = Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM;
		int num_actions = selected_animation.m_letter_actions.Count;
		float gui_y_offset = WINDOW_BORDER_TOP;
		int action_idx = 0;
		
		gui_y_offset = WINDOW_BORDER_TOP;
		
		// Draw A's
		m_state_node_rects = new Rect[num_actions];
		
		EditorGUI.LabelField(new Rect(state_overview_x, gui_y_offset, 100, LINE_HEIGHT*2), "Loops [" + selected_animation.m_loop_cycles.Count + "]", EditorStyles.boldLabel);
		gui_y_offset += LINE_HEIGHT*2;
		
		if(GUI.Button(new Rect(state_overview_x, gui_y_offset, 60, LINE_HEIGHT), new GUIContent(m_display_loops_tree ? "Hide" : "Show", (m_display_loops_tree ? "Hide" : "Show") + " the loops setup menu.")))
		{
			m_display_loops_tree = !m_display_loops_tree;
		}
		gui_y_offset += LINE_HEIGHT * 2;
		
		
		LOOP_SCROLL_POS = GUI.BeginScrollView(
			new Rect(state_overview_x, gui_y_offset, MainEditorX - state_overview_x - 14, main_editor_height - (gui_y_offset - WINDOW_BORDER_TOP)),
			LOOP_SCROLL_POS,
			new Rect(0,0, MainEditorX - 35, selected_animation.m_letter_actions.Count * ACTION_NODE_SPACING)
		);
		
		gui_y_offset = 0;
		
		for(action_idx = 0; action_idx < selected_animation.m_letter_actions.Count; action_idx++)
		{
			GUI.Label(new Rect(20, gui_y_offset, 40, 20), "A" + action_idx, EditorStyles.boldLabel);
			
			if(m_display_loops_tree)
			{
				m_state_node_rects[action_idx] = new Rect(20 - ACTION_NODE_MARGIN, gui_y_offset - ACTION_NODE_MARGIN, 20 + (2 * ACTION_NODE_MARGIN), 20 + (2 * ACTION_NODE_MARGIN));
			}
			
			gui_y_offset += ACTION_NODE_SPACING;
		}
		
		
		if(m_display_loops_tree)
		{
			// Draw existing loops
			Vector2 point1, point2;
			
			// Draw Loop assignment-in-progress line, triggered by users click and drag on action nodes
			if(m_mouse_down_node_idx >= 0)
			{
				DrawLine(m_mouse_down_pos, new Vector2(m_mouse_down_pos.x, m_mouse_drag_pos.y), Color.gray, 3);
			}
			
			state_overview_width = 0;
			
			int num_loops = selected_animation.m_loop_cycles.Count;
			GUIStyle my_style = EditorStyles.miniButton;
			my_style.alignment = TextAnchor.MiddleLeft;
			my_style.normal.textColor = Color.black;
			
			
			// Loop through loop list, drawing loop line representations on loop timeline and adding loop list entries
			float loops_list_x;
			ActionLoopCycle loop_cycle;
			int last_span_width = -1;
			int indent_num = 0;
			float line_offset_x = 0;
			float loop_list_line_y = 0;
			
			GUI.SetNextControlName ("LoopsTitle");
			
			// Display loop list header
			EditorGUI.LabelField(new Rect(loop_tree_width + 60, loop_list_line_y, 100, LINE_HEIGHT), "Active Loops", EditorStyles.boldLabel);
			loop_list_line_y += LINE_HEIGHT;
			
			EditorGUI.LabelField(new Rect(loop_tree_width + 52, loop_list_line_y, 30, LINE_HEIGHT), new GUIContent("From","The index of the action to start this loop."), EditorStyles.miniBoldLabel);
			EditorGUI.LabelField(new Rect(loop_tree_width + 87, loop_list_line_y, 20, LINE_HEIGHT), new GUIContent("To", "The index of the action to end this loop."), EditorStyles.miniBoldLabel);
			EditorGUI.LabelField(new Rect(loop_tree_width + 110, loop_list_line_y, 42, LINE_HEIGHT), new GUIContent("#Loops", "The number of times to run through this loop. Enter zero to run loop infinitely."), EditorStyles.miniBoldLabel);
			EditorGUI.LabelField(new Rect(loop_tree_width + 190, loop_list_line_y, 40, LINE_HEIGHT), new GUIContent("Type", "The type/behaviour of this loop."), EditorStyles.miniBoldLabel);
			EditorGUI.LabelField(new Rect(loop_tree_width + 262, loop_list_line_y, 70, LINE_HEIGHT), new GUIContent("DFO [?]", "'Delay First Only' - Letter action delays (non-constant) will only be applied for the first forward pass through the loop. This stops the letters getting more and more out of sequence with every loop interation."), EditorStyles.miniBoldLabel);
			loop_list_line_y += LINE_HEIGHT;
			
			
			for(int loop_idx=0; loop_idx < num_loops; loop_idx++)
			{
				loop_cycle = selected_animation.m_loop_cycles[loop_idx];
				
				if(last_span_width == -1)
				{
					last_span_width = loop_cycle.SpanWidth;
				}
				
				// Check for invalid loops
				if(loop_cycle.m_start_action_idx >= num_actions || loop_cycle.m_end_action_idx >= num_actions)
				{
					// invalid loop. Delete it.
					selected_animation.m_loop_cycles.RemoveAt(loop_idx);
					loop_idx--;
					num_loops--;
					continue;
				}
				
				// Represent loop as a line on the Action timeline
				if(last_span_width != loop_cycle.SpanWidth)
				{
					last_span_width = loop_cycle.SpanWidth;
					indent_num++;
				}
				
				line_offset_x = 20 + (indent_num * LOOP_LINE_OFFSET);
				
				if(loop_cycle.m_start_action_idx != loop_cycle.m_end_action_idx)
				{
					point1 = m_state_node_rects[loop_cycle.m_start_action_idx].center + new Vector2(line_offset_x, 0);
					point2 = m_state_node_rects[loop_cycle.m_end_action_idx].center + new Vector2(line_offset_x, 0);
					
					DrawLine(point1, point2, loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 2);
					
					DrawLine(point1 + new Vector2(0, -2), point1 + new Vector2(0, 2), loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
					DrawLine(point2 + new Vector2(0, -2), point2 + new Vector2(0, 2), loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
				}
				else
				{
					point1 = m_state_node_rects[loop_cycle.m_start_action_idx].center + new Vector2(line_offset_x, -2);
					point2 = m_state_node_rects[loop_cycle.m_end_action_idx].center + new Vector2(line_offset_x, 2);
					
					DrawLine(point1, point2, loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
				}
				
				// Display loop number
				my_style.normal.textColor = loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR;
				EditorGUI.LabelField(new Rect(point1.x, (point1.y + point2.y) / 2 - 10, loop_cycle.m_number_of_loops > 9 ? 30 : 20, 20),  loop_cycle.m_number_of_loops <= 0 ? "~" : "" + loop_cycle.m_number_of_loops, my_style); //EditorStyles.miniButton);
				
				
				// Display list view of loop cycle
				loops_list_x = loop_tree_width;
				EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 100, LINE_HEIGHT), "Loop " + (loop_idx+1));
				loops_list_x += 58;
				EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), "" + loop_cycle.m_start_action_idx);
				loops_list_x += 30;
				EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), "" + loop_cycle.m_end_action_idx);
				loops_list_x += 32;
				loop_cycle.m_number_of_loops = EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), loop_cycle.m_number_of_loops);
				loops_list_x += 30;
				loop_cycle.m_loop_type = (LOOP_TYPE) EditorGUI.EnumPopup(new Rect(loops_list_x, loop_list_line_y, 110, LINE_HEIGHT), loop_cycle.m_loop_type);
				loops_list_x += 130;
				loop_cycle.m_delay_first_only = EditorGUI.Toggle(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), loop_cycle.m_delay_first_only);
				loops_list_x += 30;
				if(GUI.Button(new Rect(loops_list_x, loop_list_line_y, 24, LINE_HEIGHT), "x"))
				{
					selected_animation.m_loop_cycles.RemoveAt(loop_idx);
					num_loops--;
					continue;
				}
				
				loop_list_line_y += LINE_HEIGHT;
				
			}
			
			loop_list_line_y += 5;
			
			// "Add new loop" line
			loops_list_x = loop_tree_width;
			EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 100, LINE_HEIGHT), "New", EditorStyles.boldLabel);
			loops_list_x += 58;
			new_loop_from = EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), new_loop_from);
			loops_list_x += 30;
			new_loop_to = EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), new_loop_to);
			loops_list_x += 32;
			if(GUI.Button(new Rect(loops_list_x, loop_list_line_y, 40, LINE_HEIGHT), "Add"))
			{
				selected_animation.AddLoop(new_loop_from, new_loop_to, false);
				font_manager.PrepareAnimationData();
				
				new_loop_from = 0;
				new_loop_to = 0;
				
				// Force keyboard focus loss from any of the loop adding fields.
				GUI.FocusControl("LoopsTitle");
			}
			
			// Set the width of the loop tree segment
			loop_tree_width = (int) line_offset_x + BASE_LOOP_LIST_POSITION_OFFSET;
			
			// Add additional width of loop list menu
			state_overview_width = loop_tree_width + LOOP_LIST_WIDTH;
			
			
			EventType eventType = Event.current.type;
			Vector2 mousePos = Event.current.mousePosition;
			
		    if (eventType == EventType.MouseDown && mousePos.x < MainEditorX)
		    {
				int rect_idx = 0;
				foreach(Rect node_rect in m_state_node_rects)
				{
					if(node_rect.Contains(mousePos))
					{
						m_mouse_down_node_idx= rect_idx;
						m_mouse_down_pos = node_rect.center;
						m_mouse_drag_pos = m_mouse_down_pos;
					}
					
					rect_idx ++;
				}
		    }
			else if(eventType == EventType.MouseDrag)
			{
				if(m_mouse_down_node_idx >= 0)
				{
					m_mouse_drag_pos = mousePos;
					
					Instance.Repaint();
				}
			}
			else if(eventType == EventType.MouseUp)
			{
				if(m_mouse_down_node_idx >= 0 && mousePos.x < MainEditorX)
				{
					int rect_idx = 0;
					foreach(Rect node_rect in m_state_node_rects)
					{
						if(node_rect.Contains(mousePos))
						{
//							Debug.LogError("you joined : " + m_mouse_down_node_idx + " and " + rect_idx + " with button " + Event.current.button);
							
							int start, end;
							if(m_mouse_down_node_idx < rect_idx)
							{
								start = m_mouse_down_node_idx;
								end = rect_idx;
							}
							else
							{
								start = rect_idx;
								end = m_mouse_down_node_idx;
							}
							
							selected_animation.AddLoop(start, end, Event.current.button == 1);
							font_manager.PrepareAnimationData();
							
							break;
						}
						
						rect_idx ++;
					}
				}
				
				m_mouse_down_node_idx = -1;
				Instance.Repaint();
			}
		}
		else
		{
			state_overview_width = 0;
		}
		
		GUI.EndScrollView();
	}
	
	
	void DrawEffectEditorPanel()
	{
		float main_editor_width = Instance.position.width - MainEditorX - WINDOW_BORDER_RIGHT;
		float HEIGHT_OF_DEFAULT_SETTINGS_GUI_PANEL = 215;
		float main_editor_height = Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM - HEIGHT_OF_DEFAULT_SETTINGS_GUI_PANEL;
		
		float gui_y_offset = WINDOW_BORDER_TOP;
		
		ignore_gui_change = false;
		noticed_gui_change = false;
		edited_action_idx = -1;
		
		// Draw Loop Tree/Action Editor divider line
		DrawLine(new Rect(MainEditorX - 10, gui_y_offset, 0, Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM), Color.gray, 3);
		
		if(GUI.Button(new Rect(MainEditorX, gui_y_offset, 100, 20), new GUIContent(!m_previewing_anim || m_paused ? "Play" : "Pause", (!m_previewing_anim || m_paused ? "Play" : "Pause") + " this animation sequence.")))
		{
			if(m_previewing_anim)
			{
				m_paused = !m_paused;
			}
			else
			{
				m_previewing_anim= true;
				
				font_manager.PlayAnimation();
				m_paused = false;
			}
			
			m_old_time = Time.realtimeSinceStartup;
		}
		if(GUI.Button(new Rect(MainEditorX + 110, gui_y_offset, 100, 20), new GUIContent("Reset", "Reset this animation to its start state.")))
		{
			m_paused = false;
			m_previewing_anim = false;
			font_manager.ResetAnimation();
		}
		
		if(font_manager.m_master_animations == null)
		{
			return;
		}
		
		float x_offset = 110;
		int continue_count = 0;
		foreach(LetterAnimation animation in font_manager.m_master_animations)
		{
			if(animation.CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING)
			{
				x_offset += 110;
				
				if(GUI.Button(new Rect(MainEditorX + x_offset, gui_y_offset, 100, 20), new GUIContent(font_manager.m_master_animations.Count > 1 ? "Continue[" + (continue_count+1) + "]" : "Continue", "Continue animation from the current waiting state.")))
				{
					font_manager.ContinueAnimation(continue_count);
				}
			}
			continue_count ++;
		}
		
		IgnoreChanges();
		
		gui_y_offset += 25;
		
		
		float text_area_height = Mathf.Clamp(Mathf.Clamp(font_manager.m_text.Split('\n').Length, 0, 6) * TEXT_AREA_LINE_HEIGHT, LINE_HEIGHT, 6 * TEXT_AREA_LINE_HEIGHT);
		
		GUI.Label(new Rect(MainEditorX, gui_y_offset, 500, LINE_HEIGHT), new GUIContent("Text", "Sets the text of the animation."));
		font_manager.m_text = EditorGUI.TextArea(new Rect(MainEditorX + 150, gui_y_offset, main_editor_width - 150, text_area_height), font_manager.m_text);
		
		if(CheckGUIChange())
		{
			// Force redraw of text meshes and animation state
			font_manager.SetText(font_manager.m_text);
			edited_action_idx = -1;
			return;
		}
		
		gui_y_offset += text_area_height + 10;
		
		font_manager.m_animate_per = (AnimatePerOptions) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Animate Per", "Sets whether to calculate state values on a per letter, per word or per line basis."), font_manager.m_animate_per);
		gui_y_offset += LINE_HEIGHT;
		
		font_manager.m_time_type = (AnimationTime) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Time", "Sets whether the animation will use in-game time, or real world time. There's only a difference if Time.timescale != 1."), font_manager.m_time_type);
		gui_y_offset += LINE_HEIGHT;
		gui_y_offset += LINE_HEIGHT;
		
		
		if(GUI.Button(new Rect(MainEditorX, gui_y_offset, 140, LINE_HEIGHT), new GUIContent("Add Animation")))
		{
			font_manager.m_master_animations.Add(new LetterAnimation());
			
			return;
		}
		if(GUI.Button(new Rect(MainEditorX + 150, gui_y_offset, 210, LINE_HEIGHT), new GUIContent("Delete Selected Animation")))
		{
			font_manager.m_master_animations.RemoveAt(ANIMATION_IDX);
			
			if(ANIMATION_IDX >= font_manager.m_master_animations.Count)
			{
				ANIMATION_IDX = font_manager.m_master_animations.Count - 1;
			}
			
			return;
		}
		gui_y_offset += LINE_HEIGHT + 5;
		
		// Check if any animations exist to draw
		if(font_manager.m_master_animations.Count == 0)
		{
			return;
		}
		
		LetterAnimation selected_animation = font_manager.m_master_animations[0];
		int num_actions = selected_animation.m_letter_actions.Count;
		
		// Draw animation selection toolbar
		string[] animation_labels = new string[font_manager.m_master_animations.Count];
		for(int anim_idx=0; anim_idx < animation_labels.Length; anim_idx++)
		{
			animation_labels[anim_idx] = "Anim " + (anim_idx + 1);
		}
		ANIMATION_IDX = GUI.Toolbar(new Rect(MainEditorX, gui_y_offset, main_editor_width, LINE_HEIGHT), ANIMATION_IDX, animation_labels);
		gui_y_offset += LINE_HEIGHT * 1.5f;
		
		
		ANIMATION_IDX = Mathf.Clamp(ANIMATION_IDX, 0, font_manager.m_master_animations.Count - 1);
		
		selected_animation = font_manager.m_master_animations[ANIMATION_IDX];
		
		selected_animation.m_letters_to_animate_option = (LETTERS_TO_ANIMATE) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Animate On", "Specifies which letters in the text this animation will affect."), selected_animation.m_letters_to_animate_option);
		
		if(selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.CUSTOM)
		{
			gui_y_offset += LINE_HEIGHT;
			
			CUSTOM_LETTERS_LIST_POS = GUI.BeginScrollView(new Rect(MainEditorX,gui_y_offset,main_editor_width,LINE_HEIGHT*3), CUSTOM_LETTERS_LIST_POS,  new Rect(0,0,font_manager.m_text.Length * 20,40));
			
			int idx = 0;
			float spacing_offset = 0;
			foreach(char character in font_manager.m_text)
			{
				if(character.Equals(' ') || character.Equals('\n'))
				{
					spacing_offset += 20;
					continue;
				}
				GUI.Label(new Rect(idx*20 + spacing_offset,0,20,50), "" + character);
				bool toggle_state = false;
				if(selected_animation.m_letters_to_animate.Contains(idx))
				{
					toggle_state = true;
				}
				
				if(GUI.Toggle(new Rect(idx*20 + spacing_offset,20,20,50), toggle_state, "") != toggle_state)
				{
					if(toggle_state)
					{
						// Letter removed from list
						selected_animation.m_letters_to_animate.Remove(idx);
					}
					else
					{
						// Adding letter to list
						selected_animation.m_letters_to_animate.Add(idx);
					}
				}
				idx++;
			}
			
			selected_animation.m_letters_to_animate.Sort();
			
			GUI.EndScrollView();
			
			gui_y_offset += LINE_HEIGHT * 2;
			
			main_editor_height -= LINE_HEIGHT * 3;
		}
		else if(selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD || selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_LINE)
		{
			gui_y_offset += LINE_HEIGHT;
			selected_animation.m_letters_to_animate_custom_idx = EditorGUI.IntField(
				new Rect(MainEditorX + 10,gui_y_offset,main_editor_width - 10,LINE_HEIGHT),
				new GUIContent((selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD ? "Word" : "Line") + " Number"),
				selected_animation.m_letters_to_animate_custom_idx);
			
			main_editor_height -= LINE_HEIGHT * 1;
		}
		
		gui_y_offset += TOOLBAR_LETTER_HEIGHT;
		
		CheckGUIChange(0, true);
		
		if(GUI.Button(new Rect(MainEditorX, gui_y_offset, 110, LINE_HEIGHT), new GUIContent("Add Action", "Add a new action state to this animation.")))
		{
			if(selected_animation.m_letter_actions.Count > 0)
			{
				selected_animation.m_letter_actions.Add(selected_animation.m_letter_actions[selected_animation.m_letter_actions.Count-1].ContinueActionFromThis());
			}
			else
			{
				selected_animation.m_letter_actions.Add(new LetterAction());
			}
			
			font_manager.PrepareAnimationData();
			
			return;
		}
		gui_y_offset += LINE_HEIGHT + 5;
		
		
		// Calculate rough height of actions panel for scrollview.
		float panel_height = 0;
		int count = 0;
		foreach(LetterAction action in selected_animation.m_letter_actions)
		{
			panel_height += HEADER_HEIGHT;
			
			if(action.FoldedInEditor)
			{
				if(action.m_offset_from_last)
				{
					panel_height += LINE_HEIGHT * ((action.m_use_gradient ? action.m_end_vertex_colour.NumEditorLines : action.m_end_colour.NumEditorLines) + 1);
					panel_height += LINE_HEIGHT * (action.m_end_pos.NumEditorLines + 1);
					panel_height += LINE_HEIGHT * (action.m_end_euler_rotation.NumEditorLines + 1);
					panel_height += LINE_HEIGHT * (action.m_end_scale.NumEditorLines + 1);
				}
				else
				{
					panel_height += LINE_HEIGHT * ((action.m_use_gradient || action.m_use_gradient_start ? action.m_start_vertex_colour.NumEditorLines : action.m_start_colour.NumEditorLines) + (count > 0 ? 1 : 0));
					panel_height += LINE_HEIGHT * ((action.m_use_gradient || action.m_use_gradient_end ? action.m_end_vertex_colour.NumEditorLines : action.m_end_colour.NumEditorLines) + 1);
					panel_height += LINE_HEIGHT * (action.m_start_pos.NumEditorLines + (count > 0 && action.m_start_pos.m_progression == ValueProgression.Eased ? 1 : 0));
					panel_height += LINE_HEIGHT * (action.m_end_pos.NumEditorLines);
					panel_height += LINE_HEIGHT * (action.m_start_euler_rotation.NumEditorLines + (count > 0 ? 1 : 0));
					panel_height += LINE_HEIGHT * (action.m_end_euler_rotation.NumEditorLines + 1);
					panel_height += LINE_HEIGHT * (action.m_start_scale.NumEditorLines + (count > 0 ? 1 : 0));
					panel_height += LINE_HEIGHT * (action.m_end_scale.NumEditorLines + 1);
					
					panel_height += LINE_HEIGHT;
				}
				
				panel_height += LINE_HEIGHT * (action.m_position_axis_ease_data.m_override_default ? 3 : 1);
				panel_height += LINE_HEIGHT * (action.m_rotation_axis_ease_data.m_override_default ? 3 : 1);
				panel_height += LINE_HEIGHT * (action.m_scale_axis_ease_data.m_override_default ? 3 : 1);
				
				panel_height += action.m_audio_on_start_display ? (action.m_audio_on_start_delay.NumEditorLines + action.m_audio_on_start_offset.NumEditorLines + action.m_audio_on_start_volume.NumEditorLines + action.m_audio_on_start_pitch.NumEditorLines) * LINE_HEIGHT : 0;
				panel_height += action.m_audio_on_finish_display ? (action.m_audio_on_finish_delay.NumEditorLines + action.m_audio_on_finish_offset.NumEditorLines + action.m_audio_on_finish_volume.NumEditorLines + action.m_audio_on_finish_pitch.NumEditorLines) * LINE_HEIGHT : 0;
				panel_height += action.m_emitter_on_start_display ? LINE_HEIGHT * (action.m_emitter_on_start_delay.NumEditorLines + action.m_emitter_on_start_duration.NumEditorLines + action.m_emitter_on_start_offset.NumEditorLines) + (LINE_HEIGHT * 2) : 0;
				panel_height += action.m_emitter_on_finish_display ? LINE_HEIGHT * (action.m_emitter_on_finish_delay.NumEditorLines + action.m_emitter_on_finish_duration.NumEditorLines + action.m_emitter_on_finish_offset.NumEditorLines) + (LINE_HEIGHT * 2) : 0;
				
				panel_height += LINE_HEIGHT * 11;
				panel_height += LINE_HEIGHT * (action.m_delay_progression.NumEditorLines);
				panel_height += LINE_HEIGHT * (action.m_duration_progression.NumEditorLines);
			}
			
			count++;
		}
		
		MAIN_SCROLL_POS = GUI.BeginScrollView(new Rect(MainEditorX, gui_y_offset, main_editor_width + 5, main_editor_height), MAIN_SCROLL_POS, new Rect(0,0, main_editor_width - 13, panel_height));
		
		gui_y_offset = 0;
		
		IgnoreChanges();
		
		
		int action_idx = 0;
		foreach(LetterAction action in selected_animation.m_letter_actions)
		{	
			action.FoldedInEditor = EditorGUI.Foldout(new Rect(0, gui_y_offset, 100, HEADER_HEIGHT), action.FoldedInEditor, "Action " + action_idx, true, HeaderGuiStyle);
			
			if(action_idx > 0)
			{
				EditorGUI.LabelField(new Rect(100, gui_y_offset, 100, LINE_HEIGHT), "Cont. Prev?");
				action.m_offset_from_last = EditorGUI.Toggle(new Rect(175, gui_y_offset, 18, LINE_HEIGHT), action.m_offset_from_last);
			}
			else
			{
				action.m_offset_from_last = false;
			}
			
			if(GUI.Button(new Rect(200, gui_y_offset, 50, LINE_HEIGHT), new GUIContent("Add", "Add a new action after this action."), EditorStyles.toolbarButton))
			{
				selected_animation.m_letter_actions.Insert(action_idx+1, action.ContinueActionFromThis());
				font_manager.PrepareAnimationData();
				break;
			}
			if(GUI.Button(new Rect(250, gui_y_offset, 60, LINE_HEIGHT), new GUIContent("Delete", "Delete this action."), EditorStyles.toolbarButton))
			{
				selected_animation.m_letter_actions.RemoveAt(action_idx);
				font_manager.PrepareAnimationData();
				break;
			}
			if(action_idx > 0 && GUI.Button(new Rect(320, gui_y_offset, 40, LINE_HEIGHT), new GUIContent("Up", "Move this action one position upwards."), EditorStyles.toolbarButton))
			{
				selected_animation.m_letter_actions.RemoveAt(action_idx);
				selected_animation.m_letter_actions.Insert(action_idx-1, action);
				font_manager.PrepareAnimationData();
				break;
			}
			if(action_idx < num_actions - 1 && GUI.Button(new Rect(360, gui_y_offset, 40, LINE_HEIGHT), new GUIContent("Down", "Move this action one position downwards."), EditorStyles.toolbarButton))
			{
				selected_animation.m_letter_actions.RemoveAt(action_idx);
				selected_animation.m_letter_actions.Insert(action_idx+1, action);
				font_manager.PrepareAnimationData();
				break;
			}
			
			IgnoreChanges();
			
			if(GUI.Button(new Rect(410, gui_y_offset, 55, LINE_HEIGHT), new GUIContent("Reset To", "Reset the state of the animation text to the start of this actions state."), EditorStyles.toolbarButton))
			{
				font_manager.SetAnimationState(action_idx, 0);
				return;
			}
        	gui_y_offset += HEADER_HEIGHT;
			
			if(action.FoldedInEditor)
			{
				action.m_action_type = (ACTION_TYPE) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Action Type", "Denotes whether this action is for animating the text (ANIM_SEQUENCE), or for pausing the animation (BREAK)."), action.m_action_type);
				gui_y_offset += LINE_HEIGHT;
				IgnoreChanges();
				
				if(action.m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
				{
					action.m_letter_anchor = (TextAnchor) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Letter Anchor", "Defines the anchor point on each letter which the rotation and scale state values are based around."), action.m_letter_anchor);
					gui_y_offset += LINE_HEIGHT;
					CheckGUIChange();
					
					action.m_ease_type = (EasingEquation) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Ease Type", "Defines which easing function to use when lerping from the action start to end states."), action.m_ease_type);
					gui_y_offset += LINE_HEIGHT;
					IgnoreChanges();
					
					// Backwards compatiblity case. Once old use_gradient bool is false, it should never be displayed again
					if(action.m_use_gradient)
					{
						action.m_use_gradient = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), "Use Colour Gradients?", action.m_use_gradient);
						gui_y_offset += LINE_HEIGHT;
						IgnoreChanges();
					}
					
					
					if(!action.m_offset_from_last)
					{
						if(!action.m_use_gradient)
						{
							action.m_use_gradient_start = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), new GUIContent("Start Colour Gradients?", "Sets whether the start state colour is a solid colour, or a 4-way gradient of colours."), action.m_use_gradient_start);
							gui_y_offset += LINE_HEIGHT;
							IgnoreChanges();
						}
						
						if(action.m_use_gradient_start || action.m_use_gradient)
						{
							gui_y_offset += DrawVertexColourEditorGUI(action.m_start_vertex_colour, new GUIContent("Start Vertex Colours"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), action_idx > 0, true);
							CheckGUIChange(action_idx, true);
						}
						else
						{
							gui_y_offset += DrawColourEditorGUI(action.m_start_colour, new GUIContent("Start Colour"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), action_idx > 0, true);
							CheckGUIChange(action_idx, true);
						}
					}
					
					if(!action.m_use_gradient)		// Backwards compatibility check
					{
						action.m_use_gradient_end = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), "End Colour Gradients?", action.m_use_gradient_end);
						gui_y_offset += LINE_HEIGHT;
						IgnoreChanges();
					}
					
					if(action.m_use_gradient_end || action.m_use_gradient)
					{
						gui_y_offset += DrawVertexColourEditorGUI(action.m_end_vertex_colour, new GUIContent("End Vertex Colours"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), true, true);
						CheckGUIChange(action_idx, false);
					}
					else
					{
						gui_y_offset += DrawColourEditorGUI(action.m_end_colour, new GUIContent("End Colour"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), true, true);
						CheckGUIChange(action_idx, false);
					}
					
					gui_y_offset += DrawAxisEaseOverrideGUI(action.m_position_axis_ease_data, new GUIContent("Set Position Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique movement."), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0));
					
					if(!action.m_offset_from_last)
					{
						gui_y_offset += DrawPositionVector3EditorGUI(action.m_start_pos, new GUIContent("Start Position"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), action_idx > 0, true);
						CheckGUIChange(action_idx, true);
					}
					gui_y_offset += DrawPositionVector3EditorGUI(action.m_end_pos, new GUIContent("End Position"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), true, true);
					CheckGUIChange(action_idx, false);
					
					gui_y_offset += DrawAxisEaseOverrideGUI(action.m_rotation_axis_ease_data, new GUIContent("Set Rotation Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique rotation."), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0));
					
					if(!action.m_offset_from_last)
					{
						gui_y_offset += DrawVector3EditorGUI(action.m_start_euler_rotation, new GUIContent("Start Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), action_idx > 0, true);
						CheckGUIChange(action_idx, true);
					}
					gui_y_offset += DrawVector3EditorGUI(action.m_end_euler_rotation, new GUIContent("End Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), true, true);
					CheckGUIChange(action_idx, false);
					
					gui_y_offset += DrawAxisEaseOverrideGUI(action.m_scale_axis_ease_data, new GUIContent("Set Scale Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique scaling."), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0));
					
					if(!action.m_offset_from_last)
					{
						gui_y_offset += DrawVector3EditorGUI(action.m_start_scale, new GUIContent("Start Scale"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), action_idx > 0, true);
						CheckGUIChange(action_idx, true);
					}
					gui_y_offset += DrawVector3EditorGUI(action.m_end_scale, new GUIContent("End Scale"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), true, true);
					CheckGUIChange(action_idx, false);
					
					action.m_force_same_start_time = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), new GUIContent("Force Same Start?", "Forces all letters in this animation to start animating this action at the same time."), action.m_force_same_start_time);
					gui_y_offset += LINE_HEIGHT;
					
					gui_y_offset += DrawFloatEditorGUI(action.m_delay_progression, new GUIContent("Delay"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true);
					if(CheckGUIChange(-1, true))
					{
						font_manager.PrepareAnimationData();
					}
				}
				
				gui_y_offset += DrawFloatEditorGUI(action.m_duration_progression, new GUIContent("Duration"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true);
				if(CheckGUIChange(-1,true))
				{
					font_manager.PrepareAnimationData();
				}
				
				// Give a little bit of extra spacing before audio/particle settings
				gui_y_offset += 5;
				
				// AUDIO ON START SETTINGS
				action.m_audio_on_start_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.m_audio_on_start_display, "", true);
				EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 120, LINE_HEIGHT), new GUIContent("Audio OnStart"), EditorStyles.boldLabel);
				action.m_audio_on_start = (AudioClip) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 160, gui_y_offset, 170, LINE_HEIGHT), action.m_audio_on_start, typeof(AudioClip), true);
				gui_y_offset += LINE_HEIGHT;
				
				if(action.m_audio_on_start_display)
				{
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_start_delay, new GUIContent("Delay", "How much this audio clip should be delayed from the start of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_start_offset, new GUIContent("Offset Time", "How far into the audio clip should it start playing."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_start_volume, new GUIContent("Volume", "What volume should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_start_pitch, new GUIContent("Pitch", "What pitch should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
				}
				
				// AUDIO ON FINISH SETTINGS
				action.m_audio_on_finish_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.m_audio_on_finish_display, "", true);
				EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 120, LINE_HEIGHT), "Audio OnFinish", EditorStyles.boldLabel);
				action.m_audio_on_finish = (AudioClip) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 160, gui_y_offset, 170, LINE_HEIGHT), action.m_audio_on_finish, typeof(AudioClip), true);
				gui_y_offset += LINE_HEIGHT;
				
				if(action.m_audio_on_finish_display)
				{
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_finish_delay, new GUIContent("Delay", "How much this audio clip should be delayed from the end of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_finish_offset, new GUIContent("Offset Time", "How far into the audio clip should it start playing."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_finish_volume, new GUIContent("Volume", "What volume should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					gui_y_offset += DrawFloatEditorGUI(action.m_audio_on_finish_pitch, new GUIContent("Pitch", "What pitch should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
				}
				
				
				// PARTICLE EMITTER ON START SETTINGS
				action.m_emitter_on_start_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.m_emitter_on_start_display, "", true);
				EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 120, LINE_HEIGHT), "Emitter OnStart", EditorStyles.boldLabel);
				action.m_emitter_on_start = (ParticleEmitter) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 160, gui_y_offset, 170, LINE_HEIGHT), action.m_emitter_on_start, typeof(ParticleEmitter), true);
				gui_y_offset += LINE_HEIGHT;
				
				if(action.m_emitter_on_start_display)
				{
					action.m_emitter_on_start_per_letter = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Effect Per Letter?", "Denotes whether to spawn a particle effect per letter, or just one."), action.m_emitter_on_start_per_letter);
					gui_y_offset += LINE_HEIGHT;
					gui_y_offset += DrawFloatEditorGUI(action.m_emitter_on_start_delay, new GUIContent("Delay", "How much this particle effect should be delayed from the end of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					
					EditorGUI.HelpBox(new Rect(ACTION_INDENT_LEVEL_1 + 330, gui_y_offset + LINE_HEIGHT, 150, LINE_HEIGHT), "zero == \"One Shot\"", MessageType.Info);
					gui_y_offset += DrawFloatEditorGUI(action.m_emitter_on_start_duration, new GUIContent("Duration", "How long this particle effect should be played for."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					
					action.m_emitter_on_start_follow_mesh = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Follow Mesh?", "Should the particle effect move and rotate with the letter mesh its assigned to?"), action.m_emitter_on_start_follow_mesh);
					gui_y_offset += LINE_HEIGHT;
					
					gui_y_offset += DrawVector3EditorGUI(action.m_emitter_on_start_offset, new GUIContent("Position Offset", "The positional offset of the particle effect from the mid-point of the letter mesh."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
				}
				
				
				// PARTICLE EMITTER ON FINISH SETTINGS
				action.m_emitter_on_finish_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.m_emitter_on_finish_display, "", true);
				EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 120, LINE_HEIGHT), "Emitter OnFinish", EditorStyles.boldLabel);
				action.m_emitter_on_finish = (ParticleEmitter) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 160, gui_y_offset, 170, LINE_HEIGHT), action.m_emitter_on_finish, typeof(ParticleEmitter), true);
				gui_y_offset += LINE_HEIGHT;
				
				if(action.m_emitter_on_finish_display)
				{
					action.m_emitter_on_finish_per_letter = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Effect Per Letter?", "Denotes whether to spawn a particle effect per letter, or just one."), action.m_emitter_on_finish_per_letter);
					gui_y_offset += LINE_HEIGHT;
					gui_y_offset += DrawFloatEditorGUI(action.m_emitter_on_finish_delay, new GUIContent("Delay", "How much this particle effect should be delayed from the end of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					
					EditorGUI.HelpBox(new Rect(ACTION_INDENT_LEVEL_1 + 330, gui_y_offset + LINE_HEIGHT, 150, LINE_HEIGHT), "zero == \"One Shot\"", MessageType.Info);
					gui_y_offset += DrawFloatEditorGUI(action.m_emitter_on_finish_duration, new GUIContent("Duration", "How long this particle effect should be played for."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
					
					action.m_emitter_on_finish_follow_mesh = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Follow Mesh?", "Should the particle effect move and rotate with the letter mesh its assigned to?"), action.m_emitter_on_finish_follow_mesh);
					gui_y_offset += LINE_HEIGHT;
					
					gui_y_offset += DrawVector3EditorGUI(action.m_emitter_on_finish_offset, new GUIContent("Position Offset", "The positional offset of the particle effect from the mid-point of the letter mesh."), new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
				}
				
				if(CheckGUIChange(-1,true))
				{
					font_manager.PrepareAnimationData();
				}
				
				// Display help box
				if(action.m_action_type == ACTION_TYPE.BREAK)
				{
					EditorGUI.HelpBox(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT*2), "Enter a delay of zero for an infinite break.\nUse the Continue() function for progressing past an animation break.", MessageType.Info);
					gui_y_offset += LINE_HEIGHT * 2;
				}
				
				IgnoreChanges();
			}
			
			action_idx++;
		}
		
		GUI.EndScrollView();
		
		DrawLoopTree(selected_animation);
	}
	
	bool CheckGUIChange()
	{
		return CheckGUIChange(font_manager.EditorActionIdx, font_manager.EditorActionProgress == 0 ? true : false);
	}
	
	bool CheckGUIChange(int action_idx, bool start_state)
	{
		if (!ignore_gui_change && !noticed_gui_change && GUI.changed)
		{
			noticed_gui_change = true;
			edited_action_idx = action_idx;
			editing_start_state = start_state;
			
			return true;
		}
		
		return false;
	}
	
	void IgnoreChanges()
	{
		if(!ignore_gui_change && !noticed_gui_change && GUI.changed)
		{
			ignore_gui_change = true;
		}
	}
	
	public float DrawAxisEaseOverrideGUI(AxisEasingOverrideData axis_data, GUIContent label, Rect position)
	{
		axis_data.m_override_default = EditorGUI.Toggle(new Rect(position.x, position.y, 200, LINE_HEIGHT), label, axis_data.m_override_default);
		
		if(axis_data.m_override_default)
		{
			EditorGUI.LabelField(new Rect(position.x + 180, position.y, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "x :");
			EditorGUI.LabelField(new Rect(position.x + 180, position.y + LINE_HEIGHT, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "y :");
			EditorGUI.LabelField(new Rect(position.x + 180, position.y + LINE_HEIGHT * 2, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "z :");
			axis_data.m_x_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_x_ease);
			axis_data.m_y_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y + LINE_HEIGHT, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_y_ease);
			axis_data.m_z_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y + (LINE_HEIGHT * 2), ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_z_ease);
			return LINE_HEIGHT * 3;
		}
		else
		{
			return LINE_HEIGHT;
		}
	}
	
	public float DrawProgressionEditorHeader(ActionVariableProgression progression, GUIContent label, Rect position, bool offset_legal, bool unique_randoms_legal, bool bold_label = true)
	{
		float x_offset = position.x;
		float y_offset = position.y;
		if(bold_label)
		{
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label, EditorStyles.boldLabel);
		}
		else
		{
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label);
		}
		x_offset += PROGRESSION_HEADER_LABEL_WIDTH;
		
		progression.m_progression = (ValueProgression) EditorGUI.EnumPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), progression.m_progression);
		x_offset += ENUM_SELECTOR_WIDTH_SMALL + 25;
		
		if(progression.m_progression == ValueProgression.Eased)
		{
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("Function :", "Easing function used to lerp values between 'from' and 'to'."));
			x_offset += 65;
			progression.m_ease_type = (EasingEquation) EditorGUI.EnumPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), progression.m_ease_type);
			x_offset += ENUM_SELECTOR_WIDTH_MEDIUM + 10;
			
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("3rd?", "Option to add a third state to lerp values between."));
			x_offset += 35;
			progression.m_to_to_bool = EditorGUI.Toggle(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), progression.m_to_to_bool);
			
		}
		else if(progression.m_progression == ValueProgression.Random && unique_randoms_legal)
		{
			progression.m_unique_randoms = EditorGUI.Toggle(new Rect(x_offset, y_offset, 200, LINE_HEIGHT), new GUIContent("Unique Randoms?", "Denotes whether a new random value will be picked each time this action is repeated (like when in a loop)."), progression.m_unique_randoms);
		}
		y_offset += LINE_HEIGHT;
		
		if(offset_legal)
		{
			progression.m_is_offset_from_last = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Offset From Last?", "Denotes whether this value will offset from whatever value it had in the last state. End states offset the start state. Start states offset the previous actions end state."), progression.m_is_offset_from_last);
			y_offset += LINE_HEIGHT;
		}
		
		if(progression.m_progression != ValueProgression.Constant)
		{
			progression.m_override_animate_per_option = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Override AnimatePer?", "Denotes whether this state value progression will use the global 'Animate Per' setting, or define its own."), progression.m_override_animate_per_option);
			if(progression.m_override_animate_per_option)
			{
				progression.m_animate_per = (AnimatePerOptions) EditorGUI.EnumPopup(new Rect(position.x + ACTION_INDENT_LEVEL_1 + 200, y_offset, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), progression.m_animate_per);
			}
			
			y_offset += LINE_HEIGHT;
		}
		else
		{
			progression.m_override_animate_per_option = false;
		}
		
		
		return position.y + (y_offset - position.y);
	}
	
	public float DrawVertexColourEditorGUI(ActionVertexColorProgression colour_prog, GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		float y_offset = DrawProgressionEditorHeader(colour_prog, label, position, offset_legal, unique_random_legal, bold_label);
		float x_offset = position.x + ACTION_INDENT_LEVEL_1;
		
		EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), colour_prog.m_progression == ValueProgression.Constant ? "Colours" : "Colours\nFrom", EditorStyles.miniBoldLabel);
		x_offset += 60;
		
		colour_prog.m_from.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_from.top_left);
		colour_prog.m_from.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_from.bottom_left);
		x_offset += 45;
		colour_prog.m_from.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_from.top_right);
		colour_prog.m_from.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_from.bottom_right);
		
		
		if(colour_prog.m_progression != ValueProgression.Constant)
		{
			x_offset += 65;
			
			EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colours\nTo", EditorStyles.miniBoldLabel);
			x_offset += 60;
			
			colour_prog.m_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to.top_left);
			colour_prog.m_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to.bottom_left);
			x_offset += 45;
			colour_prog.m_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to.top_right);
			colour_prog.m_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to.bottom_right);
			
			
			if(colour_prog.m_progression == ValueProgression.Eased && colour_prog.m_to_to_bool)
			{
				x_offset += 65;
			
				EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colours\nThen To", EditorStyles.miniBoldLabel);
				x_offset += 60;
				
				colour_prog.m_to_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to_to.top_left);
				colour_prog.m_to_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to_to.bottom_left);
				x_offset += 45;
				colour_prog.m_to_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to_to.top_right);
				colour_prog.m_to_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to_to.bottom_right);
			}
		}
		
		return (y_offset + LINE_HEIGHT * 2 + 10) - position.y;
	}
	
	public float DrawColourEditorGUI(ActionColorProgression colour_prog, GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		float x_offset = position.x + ACTION_INDENT_LEVEL_1;
		float y_offset = DrawProgressionEditorHeader(colour_prog, label, position, offset_legal, unique_random_legal, bold_label);
		
		EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), colour_prog.m_progression == ValueProgression.Constant ? "Colour" : "Colour\nFrom", EditorStyles.miniLabel);
		x_offset += 60;
		
		colour_prog.m_from = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_from);
		
		if(colour_prog.m_progression != ValueProgression.Constant)
		{
			x_offset += 65;
			
			EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colour\nTo", EditorStyles.miniBoldLabel);
			x_offset += 60;
			
			colour_prog.m_to = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to);
			
			if(colour_prog.m_progression == ValueProgression.Eased && colour_prog.m_to_to_bool)
			{
				x_offset += 65;
			
				EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colour\nThen To", EditorStyles.miniBoldLabel);
				x_offset += 60;
				
				colour_prog.m_to_to = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), colour_prog.m_to_to);
			}
		}
		
		return (y_offset + LINE_HEIGHT + 10) - position.y;
	}
	
	public float DrawPositionVector3EditorGUI(ActionPositionVector3Progression vec_prog, GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		float x_offset = position.x + ACTION_INDENT_LEVEL_1;
		float y_offset = DrawProgressionEditorHeader(vec_prog, label, position, offset_legal, unique_random_legal, bold_label);
		
		if(vec_prog.m_progression != ValueProgression.Eased)
		{
			Rect toggle_pos = new Rect();
			if(offset_legal)
			{
				toggle_pos = new Rect(x_offset + 190, y_offset - LINE_HEIGHT, 200, LINE_HEIGHT);
			}
			else
			{
				toggle_pos = new Rect(x_offset, y_offset, 200, LINE_HEIGHT);
				
				y_offset += LINE_HEIGHT;
			}
			vec_prog.m_force_position_override = EditorGUI.Toggle(toggle_pos, "Force This Position?", vec_prog.m_force_position_override);
		}
		
		vec_prog.m_from = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), vec_prog.m_progression == ValueProgression.Constant ? "Vector" : "Vector From", vec_prog.m_from);
		y_offset += LINE_HEIGHT*2;
		
		if(vec_prog.m_progression != ValueProgression.Constant)
		{
			vec_prog.m_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector To", vec_prog.m_to);
			y_offset += LINE_HEIGHT*2;
			
			if(vec_prog.m_progression == ValueProgression.Eased && vec_prog.m_to_to_bool)
			{
				vec_prog.m_to_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector Then", vec_prog.m_to_to);
				y_offset += LINE_HEIGHT*2;
			}
		}
		
		return (y_offset) - position.y;
	}
	
	
	public float DrawVector3EditorGUI(ActionVector3Progression vec_prog, GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		float x_offset = position.x + ACTION_INDENT_LEVEL_1;
		float y_offset = DrawProgressionEditorHeader(vec_prog, label, position, offset_legal, unique_random_legal, bold_label);
		
		vec_prog.m_from = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), vec_prog.m_progression == ValueProgression.Constant ? "Vector" : "Vector From", vec_prog.m_from);
		y_offset += LINE_HEIGHT*2;
		
		if(vec_prog.m_progression != ValueProgression.Constant)
		{
			vec_prog.m_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector To", vec_prog.m_to);
			y_offset += LINE_HEIGHT*2;
			
			if(vec_prog.m_progression == ValueProgression.Eased && vec_prog.m_to_to_bool)
			{
				vec_prog.m_to_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector Then", vec_prog.m_to_to);
				y_offset += LINE_HEIGHT*2;
			}
		}
		
		return (y_offset) - position.y;
	}
	
	
	public float DrawFloatEditorGUI(ActionFloatProgression float_prog, GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		float x_offset = position.x + ACTION_INDENT_LEVEL_1;
		float y_offset = DrawProgressionEditorHeader(float_prog, label, position, offset_legal, unique_random_legal, bold_label);
		
		float_prog.m_from = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), float_prog.m_progression == ValueProgression.Constant ? "Value" : "Value From", float_prog.m_from);
		y_offset += LINE_HEIGHT;
		
		if(float_prog.m_progression != ValueProgression.Constant)
		{
			float_prog.m_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value To", float_prog.m_to);
			y_offset += LINE_HEIGHT;
			
			if(float_prog.m_progression == ValueProgression.Eased && float_prog.m_to_to_bool)
			{
				float_prog.m_to_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value Then", float_prog.m_to_to);
				y_offset += LINE_HEIGHT;
			}
		}
		
		return (y_offset) - position.y;
	}
	
	
	public static Texture2D lineTex;
 
    public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }
    public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }
    public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }
    public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
		if(pointA.Equals(pointB))
		{
			// points are the same.
			return;
		}
		
        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;
 
        // Generate a single pixel texture if it doesn't exist
        if (!lineTex) { lineTex = new Texture2D(1, 1); }
 
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        Color savedColor = GUI.color;
        GUI.color = color;
 
        // Determine the angle of the line.
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);
 
        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) { angle = -angle; }
 
        // Use ScaleAroundPivot to adjust the size of the line.
        // We could do this when we draw the texture, but by scaling it here we can use
        //  non-integer values for the width and length (such as sub 1 pixel widths).
        // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
        //  is centered on the origin at pointA.
        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
 
        // Set the rotation for the line.
        //  The angle was calculated with pointA as the origin.
        GUIUtility.RotateAroundPivot(angle, pointA);
 
        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper width, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
 
        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
}