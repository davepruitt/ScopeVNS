   M S B A M L   `   `   `  ���� C  ?ScopeVNS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:ScopeVNSScopeVNS      ScopeVNS.Booth   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:ScopeVNS     R NOxyPlot.Wpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  -     � Title$  ScopeVNS��$	��500��$	��525��.���  � RowDefinitions �� $��50 ��� $��30 ��� $��* ��� $	��150 � � ColumnDefinitions �� $	��150 ��� $	��175 ��� $	��150 ��� $��10 ��� $��* �.+��� $��
Rat Name: ��$��0��$��0��$
��Left=�$	��Top=�$	��Top=�$��28(�$
��Bold"�$
��Left=��� -   $��RatNameTextBox��$��0��$��1��$
��Left=�$��Center=�$	��160��$	��5,0q�$��28(�$
��Bold"����� *	RatName+ �� Mode$ TwoWay=��� -   $��Center=�$��Center=�$	��100��$��0��$��2��$
��Bold"�$��18(�����  �� Path$ StartButtonText� �� FallbackValue$ Start��{��� *StartButtonEnabled+���� *StartButtonTextColor+�� $��Booth: ��$��1��$��0��$
��Left=�$	��Top=�$	��Top=�$��18(�$
��Bold"�$
��Left=��� $��1��$��1��$
��Left=�$	��Top=�$	��Top=�$��18(�$
��Bold"�$
��Left=����� *BoothNumber+�� $��
Scope ID: ��$��1��$��2��$
��Left=�$	��Top=�$	��Top=�$
��Left=��� $��1��$��3��$
��Left=�$	��Top=�$	��Top=�$
��Left=����� *	ScopeID+  OxyPlot.Wpf.PlotView  $��2��$��0��$��5��   Model �� *BoothPlotModel+� $��3��$��0��$��5�� �� $��30 ��� $��* �.+��� $��0��$��	Messages:���� -   $��MessagesTextBox�� �� TextWrapping$
 Wrap=� �� AcceptsReturn . $��2q�$��1��"	 �� VerticalScrollBarVisibility$	 Visible=�$	��100��
 �� 
IsReadOnly
 . ���� *
Messages+$ TwoWay=�