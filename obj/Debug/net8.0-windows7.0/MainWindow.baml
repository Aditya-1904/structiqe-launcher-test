   M S B A M L   `   `   `  ����X  TstructIQe Application Manager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullu?clr-namespace:structIQe_Application_Manager.Launcher.Converters1structIQe_Application_Manager.Launcher.Converters  5   	   7    1structIQe_Application_Manager.Launcher.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Pconv?clr-namespace:structIQe_Application_Manager.Launcher.Converters     5   	   $��
Swis721 BT)�5   	     � 
ResizeMode$  NoResize=�5   	   $	��700��5   	   $
��1005��6    � Title$# structIQe Application Manager��5   	   $
��1000��5
   	   $	��600��6    � 	Resources 5      Q   KstructIQe_Application_Manager.Launcher.Converters.ActionButtonTextConverter   
ActionTextQ   KstructIQe_Application_Manager.Launcher.Converters.BoolToVisibilityConverter  	BoolToVisN   HstructIQe_Application_Manager.Launcher.Converters.BoolToOpacityConverter  BoolToOpacity��%W   &	        &	      &	 &       5   
   6)     5   
   6)     5   
   6&   5      .��5      � ���#EEE � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   � $��0��6   ���#EEE$	��100��6.    � ColumnDefinitions 5       �� 5!      $
��Auto �6#   �� 5"      $��* �6#   �� 5#      $
��Auto �6#   .+�5'      �� $��0��6    �� Orientation$ Vertical=�6)   $��Center=�6@   $��	24,0,12,0q�6[   .+�5(      �� $	��Top=�6   $��28(�64   $
��Bold"�6B   Application Manager5*      �� 5+      .+�5,      �� $��Version 1.1.0��6    �� 5-      $��Copyright © 2025��6    �� 5.      $$��structIQe Technologies Pvt Ltd��6    51      52      �� 55      $��1��6   �� 58      $��2��6   $ 
Horizontal=�6)   $��Center=�6B   $��0,0,24,0q�6]   .+�59      �� -   5        $��Request/Update License��59      ���#ffffff$��25��6h   $	��145��6t   $
��10,0q�6�    � Cursor$
 Hand��6�   �� 5:      -   5        $��Apply License��5:      ���#ffffff$��25��6\   $	��120��6h   $
��10,0q�6t   $
 Hand��6�   5R      5S      � 5V   
   $	��150��6   $��1��6    5W      �� 5X      $��24 �6#   �� 5Y      $	��220 �6#   �� 5Z      $��24 �6#   �� 5\      .+�5a      �� $��1��6   ���#EEE$��1q�67   ���#ddd._�5b      �� $ ��/Images/Vertical_image.png��6   $��Center=�5c      $��Center=�5d      $
��Fill=�5e      $��0,0,0,0q�5f      5g      �� 5j      -   5         ��Name$ AppsListView��5j      $��3��5k      ���#ddd$��0,0,24,0q�5m   /    �� SelectionMode$ Single=�6A   ��6   �� ��5n      �� 5o      	 �� 
TargetType"	 ��6   .�5p      �� 
 �� Property
 � �� �� Value$	 0,8q�63   �� 5q      
 � ��$ 6q�64   �� 5r      
 � ��$ 0q�6<   �� 5s      
 � ��$ Stretch=�6G   �� 5t      
 � ��$	 Top=�6E   �� 5u      
 � ��
 ���� 67   �� 5v      
 �@�Cursor$
 Hand��63   5y       �� View 5|      �� 5}      .P�5      �� $
��Name��6)    �� CellTemplate 5�      �� 5�   "   .R�5�   &   �� .+�5�   &   �� $
��Bold"�6F   $��15(�6X   ��60   �� *Name+6X   �� 5�   *   ��67   �� *	Modules+��5�   .   �� 5�   2   .R�5�   6   �� $��11(�5�   8   ���#666$��10,0,0,0q�5�   8   ��5�   @   �� 5�   8   5�   3   5�   +   5�   '   5�   #   5�      �� 5�      $��Current Version��6)    5�      �� 5�   "   .R�5�   &   �� $��Center=�5�   0   $��Center=�5�   0    �� TextAlignment$ Center=�5�   0   ��5�   0   �� *InstalledVersion+ �� TargetNullValue$ Not Installed��5�   0   5�   #   5�      �� 5�      $��Action��6)    5�      �� 5�   "   .R�5�   &   �  J� Opacity 6+   �� *
IsLicensed+ �� 	Converter# [  J� IsHitTestVisible 5�      �� *
IsLicensed+.+�5�   *   �� .+�5�   .   �� -   5        $��30��5�   	   $
 Hand��5�   	   ��5�   5   �� *.+# [  {�5�   	   �� *CanInstallLicensed+ �� Mode$ OneWay=�
 � Tag 5�   	   �� *Id+��5�   2   �� 5�   6   "	 ��6<   .�5�   :   �� 
 � �� �#F5FAFF6W   �� 5�   :   
 � �� �#1967D26W    �� Triggers 5�   :   �� 5�   >    �� Binding 6J   �� *IsInstalled+ �� Value True6j   .a�5�   B   �� 
 � �� �#6666666_   5�   ?   �� 5�   >    6J   �� *IsUpdateAvailable+ True6p   .a�5�   B   �� 
 � �� �#1967D26_   5�   ?   5�   7   5�   /   �� 5�   .   $��0,2,0,0q�69   .+�5�   2   �� $��10(�5�      
���������5�   <   �� *LatestVersion+ �� StringFormat$ Available Version: {0}����5�   6   �� 5�   :   "	 ��6@   .�5�   >   �� 
 � y�$ 	Collapsed=�6[    5�   >   ^� 5�   B    ^� 
Conditions 5�   F   �� 5�   J    �� Binding 6T   �� *IsInstalled+ �� Value	 False6t   .0�5�   F   �� 
 � y�$ Visible=�6c   5�   C   �� 5�   B    6N   �� *IsUpdateAvailable+ True6t   .a�5�   F   �� 
 � y�$ Visible=�6c   5�   C   5�   ;   5�   3   5�   /   �� 5�   .   $��0,2,0,0q�69   ��5�   2   �� 5�   6   "	 ��6<   .�5�   :   �� 
 � y�$ 	Collapsed=�6W    5�   :   �� 5�   >    6J   �� *IsUpdateAvailable+ True6p   .a�5�   B   �� 
 � y�$ Visible=�6_   5�   ?   5�   7   .+�5�   3   �� $��10(�5�   >   ���#666$
��Wrap=�6^   $	��220��6r   ��5�   =   �� *ReleaseNotes+5�   r   5�   /   5�   +   5�   '   5�   #   5�      �� 5�      $��	Uninstall��6)    5�      �� 5�   "   .R�5�   &   � $	��Top=�5�       5�   +   �� *
IsLicensed+# [  5�      �� *
IsLicensed+.+�5�   *   �� -   5        $��	Uninstall��5�   1   $	��Top=�5�   1   ���#ffffff$��30��5  1   $	��8,0q�5  1   $
 Hand��5  1    5�   1   �� *Id+{�5   1   �� *IsInstalled+��5  .   �� 5  2   "	 ��68   .�5  6   �� 
 � ��
 �  ��6S    5  6   P� 5	  :    P� Property � {� P� Value$ False��6W   . �5
  >   �� 
 � ��
 �����6[   5  ;   5  3   5  +   5  '   5  #   5     5     5     5     ]� 5  
   $��2��6   $��False��6!   $��54��67   ���#EEE.+�5     �� -   5         ��Name$ UninstallAllButton��5     $��Uninstall All��5   	   $	��140��5!  	   $��30��5"  	   $��24,8,0,8q�5#  	   $
��Left=�5$  	   ���#ffffff��5'     �� 5(     "	 ��6   .�5)     �� 
 � ��
 �  ��67    5*     P� 5+      � {�$ False��6;   . �5,  "   �� 
 � ��
 �����6?   5-     5/     51     �� 53     -   5        $��Close��53     $	��100��55     $��30��56     $��0,8,24,8q�57     $��Right=�58     ���#ffffff
���  ��5<     5=     �� 5@  
   -   5         ��Name$ LoadingOverlay��5@     ���	#AAFFFFFF$y�	Collapsed=�5B       �� CornerRadius$  12��5C     $��3��5D     $��0q�5E     ! K� ZIndex$	! 999��5F     ._�5G     �� $��Center=�6   $��Center=�64   .+�5H     � " � IsIndeterminate$
" True��6   $	��200��65   $��24��6A   $��0,0,0,16q�6M   �� 5I     $(��"Checking for the latest updates...��6   $��16(�6F   $��SemiBold"�6T   $��Center=�6j   5J     5K     5L     5M     