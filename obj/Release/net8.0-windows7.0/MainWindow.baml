   M S B A M L   `   `   `  ���� X  TstructIQe Application Manager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullu?clr-namespace:structIQe_Application_Manager.Launcher.Converters1structIQe_Application_Manager.Launcher.Converters  7    1structIQe_Application_Manager.Launcher.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Pconv?clr-namespace:structIQe_Application_Manager.Launcher.Converters     $��
Swis721 BT)�  � 
ResizeMode$  NoResize=�$	��700��$
��1005�� � Title$# structIQe Application Manager��$
��1000��$	��600�� � 	Resources Q   KstructIQe_Application_Manager.Launcher.Converters.ActionButtonTextConverter   
ActionTextQ   KstructIQe_Application_Manager.Launcher.Converters.BoolToVisibilityConverter  	BoolToVisN   HstructIQe_Application_Manager.Launcher.Converters.BoolToOpacityConverter  BoolToOpacity��%-   &	        &	      &	 
           .��� ���#EEE � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+�� $��0�����#EEE$	��100�� � ColumnDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0�� �� Orientation$ Vertical=�$��Center=�$��	24,0,12,0q�.+��� $	��Top=�$��28(�$
��Bold"�Application Manager�� .+��� $��Version 1.1.0���� $��Copyright © 2025���� $$��structIQe Technologies Pvt Ltd���� $��1���� $��2��$ 
Horizontal=�$��Center=�$��0,0,24,0q�.+��� -   $��Request/Update License�����#ffffff$��25��$	��145��$
��10,0q� � Cursor$
 Hand���� -   $��Apply License�����#ffffff$��25��$	��120��$
��10,0q�$
 Hand��� $	��150��$��1�� �� $��24 ��� $	��220 ��� $��24 ��� .+��� $��1�����#EEE$��1q����#ddd._��� $ ��/Images/Vertical_image.png��$��Center=�$��Center=�$
��Fill=�$��0,0,0,0q��� -    ��Name$ AppsListView��$��3�����#ddd$��0,0,24,0q� �� SelectionMode$ Single=����� ���� 	 �� 
TargetType"	 ��.��� 
 �� Property
 � �� �� Value$	 0,8q��� 
 � ��$ 6q��� 
 � ��$ 0q��� 
 � ��$ Stretch=��� 
 � ��$	 Top=��� 
 � ��
 ���� �� 
 �@�Cursor$
 Hand�� �� View �� .P��� $
��Name�� �� CellTemplate �� .R��� .+��� $
��Bold"�$��15(����� *Name+�� ���� *	Modules+���� .R��� $��11(����#666$��10,0,0,0q����� �� $��Current Version�� �� .R��� $��Center=�$��Center=� �� TextAlignment$ Center=����� *InstalledVersion+ �� TargetNullValue$ Not Installed���� $��Action�� �� .R��  J� Opacity �� *
IsLicensed+ �� 	Converter# [  J� IsHitTestVisible �� *
IsLicensed+.+��� .+��� -   $��30��$
 Hand������ *.+# [  {��� *CanInstallLicensed+ �� Mode$ OneWay=�
 � Tag �� *Id+���� "	 ��.��� 
 � �� �#F5FAFF�� 
 � �� �#1967D2 �� Triggers ��  �� Binding �� *IsInstalled+ �� Value True.a��� 
 � �� �#666666��  �� *IsUpdateAvailable+ True.a��� 
 � �� �#1967D2�� $��0,2,0,0q�.+��� $��10(�
����������� *LatestVersion+ �� StringFormat$ Available Version: {0}������ "	 ��.��� 
 � y�$ 	Collapsed=� ^�  ^� 
Conditions ��  �� Binding �� *IsInstalled+ �� Value	 False.0��� 
 � y�$ Visible=���  �� *IsUpdateAvailable+ True.a��� 
 � y�$ Visible=��� $��0,2,0,0q����� "	 ��.��� 
 � y�$ 	Collapsed=� ��  �� *IsUpdateAvailable+ True.a��� 
 � y�$ Visible=�.+��� $��10(����#666$
��Wrap=�$	��220������ *ReleaseNotes+�� $��	Uninstall�� �� .R�� $	��Top=� �� *
IsLicensed+# [  �� *
IsLicensed+.+��� -   $��	Uninstall��$	��Top=����#ffffff$��30��$	��8,0q�$
 Hand�� �� *Id+{��� *IsInstalled+���� "	 ��.��� 
 � ��
 �  �� P�  P� Property � {� P� Value$ False��. ��� 
 � ��
 �����]� $��2��$��False��$��54�����#EEE.+��� -    ��Name$ UninstallAllButton��$��Uninstall All��$	��140��$��30��$��24,8,0,8q�$
��Left=����#ffffff���� "	 ��.��� 
 � ��
 �  �� P�  � {�$ False��. ��� 
 � ��
 ������� -   $��Close��$	��100��$��30��$��0,8,24,8q�$��Right=����#ffffff
���  ���� -    ��Name$ LoadingOverlay�����	#AAFFFFFF$y�	Collapsed=�  �� CornerRadius$  12��$��3��$��0q�! K� ZIndex$	! 999��._��� $��Center=�$��Center=�.+�� " � IsIndeterminate$
" True��$	��200��$��24��$��0,0,0,16q��� $(��"Checking for the latest updates...��$��16(�$��SemiBold"�$��Center=�