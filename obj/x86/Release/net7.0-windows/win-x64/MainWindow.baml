   M S B A M L   `   `   `  ���� D  @TxtReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:TxtReader	TxtReader      TxtReader.MainWindow   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:TxtReader       � Title$  TXT阅读器��$	��450��$	��800��.��]� $
��True��.+�c� $	��Top=�.@��� -    ��Name$ btnCloseLeft��$
��👈���� -   $��导入���� -   $��打开���� -   $��保存���� -   $��帮助���� $��1���� -   $
��💰���� $��1���� -   $ btnCloseRight��$
��👉���� $��Bottom=�.@��� -    ��Name$ tbStatusEncoding��$��0 0 5 0q��� $��1���� -	   $ tbStatusSecSchedule��$��5 0 5 0q��� $��1���� -
   $ tbStatusTxtSchedule��$��5 0 5 0q��� $��1���� -   $ tbStatusPerSchedule��$��5 0 5 0q��� $��1��A� -    A�Name$ ufgLeft��$
��Left=� A� Columns$ 1������ *Value+ �� ElementName$ asLeftWidth��.+�R� -    R�Name$ tvBookShelf��$
��Auto=�$
��Auto=��� -    ��Name$ 	lvCatalog��$
��Auto=�$
��Auto=�]� -    ]�Name$ svRight��$
��True��$��Right=����� *Value+$ asRightWidth��.+��� $	��Top=�	 � 	Resources	 
 �� 
TargetType �� Property �� Value��%�   '��       '�� "     '�� D     '�� �     �� "
 ��.���  � ��$ 4q��� "
 ��.���  � ��$ 4q��� "
 ��.���  � ��$ 4q���  � ��$ Center=��� "
 ��.���  � ��$ 4q���  � ��$ Center=�.����  �� Orientation$ Vertical=�.+��� $��阅读控制��.���� $ 
Horizontal=�.+��� -    ��Name$ 
cbReadMode��A� $ 5��.+��� -   $��⏮️���� -   $	��⏪���� -   $��▶️���� -   $	��⏩���� -   $��⏭️���� -   $
��📈��$��25 5 0 5q��� -   $	��❌��9� $	��Top=�$��阅读设置��.���� .+�   TxtReader.AdvanceSlider  -     Name$ 
sReadSpeed��   TxtReader.AdvanceSlider   ValueChanged$! AdvancedSlider_ValueChanged��   Title$ 滚动延时��   Unit$ ms��   
TitleWidth$ 50Q�   	UnitWidth$ 25Q�
   Min$	 100Q�
   Max$
 5000Q�   Value$	 800Q�  -   $ 	sReadLine��$! AdvancedSlider_ValueChanged��$ 每次滚动��$	 行��$ 50Q�$ 25Q�$ 1Q�$ 30Q�$ 1Q�  -   $ sSoundVolume��$ sSoundVolume_ValueChanged��$ 朗读音量��$ 50Q�$	 100Q�$ 0Q�$ 25Q�  -   $ sSpeechRate��$ sSpeechRate_ValueChanged��$ 朗读语速��$ 50Q�$ 10Q�$	 -10Q�$ 0Q�{��� *	IsEnabled+$ sSoundVolume���� $ 
Horizontal=�{��� *	IsEnabled+$ sSoundVolume��.+��� $��朗读音源��$��50���� -   $ cbSoundCulture��$	��100���� -   $ cbSoundSource��$	��150��9� $	��Top=�$��外观设置��.���� .+��� $ 
Horizontal=�.+��� $��字体���� -   $ 
cbFontArea��$	��100���� -   $ cbFont��$	��150��A� $ 3��.+��� -    $ cbFontWeight���� -!   $ cbFontStrech���� -"   $ cbFontStyle��  -#   $ 	sFontSize��$ 尺寸��$	 100Q�$ 1Q�$ 15Q�  -$   $ sLineHeight��$ 行高��$ 10Q�$ 1Q�$ 2Q�$y�	Collapsed=��� -%    ��Name$ color��$ 
Horizontal=�.+��� $��前景���� -&   $ cbForeColor��$	��105���� $��背景���� -'   $ 	cbBgColor��$	��105��  -(   $ asTextOpacity��$ 	透明度��   IsSnapToTickEnabled$ False��$ 40Q�$ 0Q�$ 1Q�$	 0.9Q�  -)   $ asLeftWidth��$ 左宽��$ 0Q�$	 500Q�$	 200Q�  -*   $ asRightWidth��$ 右宽��$	 200Q�$	 500Q�$	 320Q�9� $	��Top=�$��文本设置��.���� -+    ��Name$ tcTextSetting��.@��� $��文本分页��.���� .+�A� $ 3��.+�	� -,    	�Name$ 	rbSecMode��$��章节模式�� d� 	IsChecked$
 TrueY�	� --   $ 
rbPageMode��$��分页模式��	� -.   $ 	rbSecPage��$��章节分页��  -/   $ asPageLines��$ 分页行数��$ 50Q�$ 0Q�$
 1000Q�$ 0Q��� $ 
Horizontal=�.+�    TxtReader.TextBoxWithLabel  -0     Name$ tbwlLF��   Label$ 	换行符��$	��150���� -1   $ btnSetSplitPages��$��设置��$��60���� $��标题设置��.���� -2   $ spRegCatalog���� $��文本优化��	 ��%B   '��       �� "
 ��.���  � ��$ 50����  � ��$ 4q�.���� .+�]� $
��True��.+��� -3   $��执行��$��Right=�A� -4   $ ufgCheckBoxes��$ 3��]� $
��True��.+��� -5    ��Name$ chkRegex��$��正则表达式��$��5q�$��Right=�  -6   $ 	tbOldText��$ 文本替换��]� $
��True��.+��� -7   $��替换��$��Right=�  -8   $ 	tbNewText��$ ➠➠➠➠���� $ 
Horizontal=�.+��� -9   $��	转繁体���� -:   $��	转简体��]� $
��True��.+��� $��编码更改��$��47���� -;   $��另存��$��Right=��� -<   $��重载��$��Right=��� -=   $ 
cbEncoding���� $��文本搜索��.���� .+�]� $
��True��.+��� ->   $��Right=�$��搜索��$��60���� -?     ��Name$  txtForSearching���� -@   $ lvSearchResult��$	��150��$
��Auto=�$
��Auto=�9� $��杂项��$	��Top=�.���� .+�A� $ 4��.+��� -A   $��保存参数���� -B   $��加载参数���� $��保存书架���� $��加载书架���� -C   $��阅读背景��A� $ 2��.+��� -D   $ chkOpenImport��$��开启时导入设置��$
 TrueY�$��3q��� -E   $ chkCloseExport��$��关闭时保存设置��$
 TrueY�$��3q��� -F   $ chkOpenLoadTxt��$��开启时导入文件��$
 TrueY�$��3q��� -G   $ chkCloseSaveTxt��$��关闭时保存文件��$
 TrueY�$��3q��� $��	信息框��.���� -H   $  txtInfo��! �� AcceptsReturn$
! True��" �� TextWrapping$
" Wrap=�"# �� VerticalScrollBarVisibility$
# Auto=�� -I   $ �Name$$ 	pgBarText��$��Bottom=�$��2���� -J   % ��Name$% gbNote��$��笔记��$��Bottom=�$	��200��$y�	Collapsed=�.��]� $
��True��.+�A� & A� Rows$& 1��$��Bottom=�.+��� -K   $��保存���� -L   $��关闭���� -M   $  tbNote��$
! True��$
" Wrap=����� *SelectedItem+$ cbFont������ *Value+$ 	sFontSize���� -N   $ tbBooks��.@��� $��	无文件��