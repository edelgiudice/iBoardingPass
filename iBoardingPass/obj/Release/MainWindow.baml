   M S B A M L   `   `   `  ����   easyGate    iBoardingPass.MainWindow   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    8x,http://schemas.microsoft.com/winfx/2006/xaml    -     �Name$  MainWindow1�� � Title$ easyGate��$��1090.94��$��1283.439�� � 
ResizeMode$ NoResize=� � Topmost .  � WindowStartupLocation$ CenterScreen=� � WindowState$ 	Maximized=�$��18(� � Icon$ Risorse/iBoardingPass.ico��.��� $��
0,0,-6,-18q� � ColumnDefinitions �� $
��839* ��� $��3* ��� $
��142* �.+��� $��10,10,0,32.835q����	#FF65C7D6$
��Left=�$��252.239��.+��� $��60�� �� Left$ 49��
	 �� Top$	 7.667��$��142.667���� -   
 ��Name$
 	gbScanner��$��BarCode Scanner��$��30.896,0,0,195.106q�$��89.148��$��Bottom=�$
��Left=�$��206.053��.���� -    ��Name$
 tLed��$
��Wrap=�$��PRONTO��$��0,10,0,0q����	#FFC9CFC9 � Cursor$
 None�� �� TextAlignment$ Center=�
�������$��46.267��$	��Top=��� $��134.328��$��10,0,0,48.806q�$��Bottom=�$��Risorse/SOGAER_logo.png��$
��Left=�$��238.806���� $
��Left=�$��	54,35,0,0q�$��164.066��$'��!Risorse/iBardingPass_icon_256.png��$	��176��$	��Top=��� -   $
 gDataOra��$��
Data & Ora��$
��Left=�$��30.896,343,0,0q�$��206.053��$��104.94��$	��Top=�.��� $��0,0,0,5q�.+��� -   $ tbData��$��
10,10,10,0q�$
��Wrap=�$��37.94��$	��Top=�$��-- /--/ ----��$ Center=�{�.  �� -   $ tbOra��$��10,38.94,10,0q�$
��Wrap=�$��00:00:00��$ Center=�{�.   J� RenderTransformOrigin$ 0.472,-0.104&��� -   $
 gBoardingPassData��$��Dati Carta di Imbarco��$
��Left=�$��
300,10,0,0q�$	��Top=�$��315.215��$��459.053��.��� $��0,0,-2,0q�.+��� -    ��Name$ lbVolo��$��Volo n.��$
��Left=�$��	34,10,0,0q�$	��Top=��� -	    ��Name$ ledFlightNumb�����	#FFF4F4F5$
��Left=�$��36��$��	10,10,0,0q�
���   �$	��Top=�$��15���� -
   $ tbFlightNum��$
��Left=�$��
115,15,0,0q�$
��Wrap=�$��
----------��$	��Top=��� -   $ ledFlightDate�����	#FFF4F4F5$
��Left=�$��36��$��	10,61,0,0q�
���   �$	��Top=�$��15���� -   $ ldFlightDate��$��
Data Volo:��$
��Left=�$��34,63.06,0,0q�$	��Top=��� -   $ tbFlightDate��$
��Left=�$��151,68.06,0,0q�$
��Wrap=�$��
--/--/----��$	��Top=��� -   $ ledAeroportoPartenza�����	#FFF4F4F5$
��Left=�$��36��$��
10,114,0,0q�
���   �$	��Top=�$��15���� -   $ 	lFromCity��$��Aeroporto Partenza��$
��Left=�$��
34,114,0,0q�$	��Top=��� -   $ tbCodFromCity��$
��Left=�$��253,124,0,0q�$
��Wrap=�$	��---��$
��69.6��$��28.94��$	��Top=��� -   $ lToCity��$��Aeroporto Destinazione��$
��Left=�$��34,152.94,0,0q�$	��Top=��� -   $ tbCodToCity��$
��Left=�$��253,157.94,0,0q�$
��Wrap=�$	��---��$
��69.6��$��28.94��$	��Top=��� -   $ lAirCompany��$��Compagnia Aerea��$
��Left=�$��34.95,196.88,0,0q�$	��Top=��� -   $ tbAirCmpany��$
��Left=�$��253,196.88,0,0q�$
��Wrap=�$	��---��$
��69.6��$��28.94��$	��Top=��� -   $ lPassengerName��$��
Passeggero��$
��Left=�$��34.95,235.82,0,0q�$	��Top=��� -   $ tbPassengerName��$
��Left=�$��151,240.82,0,0q�$
��Wrap=�$��--------------------��$��276.6��$��28.94��$	��Top=��� -    ��Name$ dummyBox��$
��Left=�$��30��$��284,473.94,0,0q� �� TextWrapping$
 Wrap=�$#��DUMMY CONTROL FOR BAR SCANNER��$	��Top=�$��0���� -   $
 gbFastTrack��$��
Fast Track��$��
791,10,0,0q�$��3��$��185.765��$	��Top=�$
��Left=�$��169.806��.���� -    ��Name$ imgFastTrack��$
��Left=�$��154.825��$	��Top=�$��157.806��$2��,pack://siteoforigin:,,,/Resources/FT_OFF.png���� -   $
 gPostazione��$��Dati Postazione��$
��Left=�$��30.896,217.45,0,0q�$��206.053��$��107.765��$	��Top=�.��� $��	0,0,-2,-2q�.+��� -    ��Name$ cbStazionePrincipale��$��Ingresso Coda��$
��Left=�$��	10,10,0,0q�$	��Top=�{�.  �� -   $ cbFastTrack��$��
Fast Track��$
��Left=�$��10,38.94,0,0q�$	��Top=�{�.  �� -   $
 gFlightDetails��$��Dettagli Volo��$
��Left=�$��300,343,0,0q�$	��Top=�$��174.94��$��459.053��.��� .+��� $��Orario Schedulato��$
��Left=�$��45,20.06,0,0q�$	��Top=��� -   $ TBOrarioPrevisto��$
��Left=�$��210,25.06,0,0q�$
��Wrap=�$��00:00��$	��Top=��� $��Orario Previsto��$
��Left=�$��45,63.12,0,0q�$	��Top=��� -   $ TBOrarioEffettivo��$
��Left=�$��210,73.12,0,0q�$
��Wrap=�$��00:00��$	��Top=��� $��
Stato Volo��$
��Left=�$��45,105.06,0,0q�$	��Top=��� -    $ TBStatoVolo��$
��Left=�$��210,110.06,0,0q�$
��Wrap=�$��------------------------��$	��Top=��� -!   $ ledFlightTime�����	#FFF4F4F5$
��Left=�$��36��$��10,61.06,0,0q�
���   �$	��Top=�$��15��$ 0.733,1.806&��� -"   $ ledFlightStatus�����	#FFF4F4F5$
��Left=�$��36��$��10,102.06,0,0q�
���   �$	��Top=�$��15��$ 0.733,1.806&��� -#    ��Name$ bnManualEnter��$��Avanti��$
��Left=�$��300,538,0,0q�$	��Top=�$��449.053��$��143.94�����	#FFD8141D$
��Bold"�$��36(����	#FFE9F3FF�� $��1038.567,10,10,32.835q�$��3�����	#FF65C7D6.+��� -$   $
 gStatistiche��$��Statistiche��$
��Left=�$��206.053��$��258.746��$��Bottom=�$ 21.499��$	 25.94��.��� $��
0,0,-12,-2q�$��270.806��$	��Top=�.+��� -%   $ lOperativoDa��$��Num. Minuti��$
��Left=�$��	10,10,0,0q�$	��Top=��� -&   $ tbMinuti��$��31.94��$��0,12,14.667,0q�$
 Wrap=�$	��Top=�$��Right=�$��61.377��$��0��{�.   �� TextAlignment$ Right=��� -'   $ lbNumLetture��$��Num. Letture��$
��Left=�$��10,43.94,0,0q�$	��Top=��� -(   $ 	tbLetture��$��31.94��$��0,48.94,14.667,0q�$
 Wrap=�$	��Top=�$��Right=�$��61.377��$ Right=�{�.  $��0���� -)   $ bnAzzeraStat��$��Azzera Statistiche��$��10,176.866,0,0q�$	��Top=�$��45.94��$
��Left=�$��165.5���� -*   $ 
lnIngressi��$��Num. Ingressi��$
��Left=�$��4.259,86.82,0,0q�$	��Top=��� -+   $ tbNumIngressi��$��31.94��$��0,86.82,14.667,0q�$
 Wrap=�$	��Top=�$��Right=�$��61.377��$ Right=�{�.  $��0���� -,   $ lnIngressi_Copy��$��Num. Respinti��$
��Left=�$��5.063,125.76,0,0q�$	��Top=��� --   $ tbNumRespinti��$��31.94��$��0,127.76,14.667,0q�$
 Wrap=�$	��Top=�$��Right=�$��61.377��$ Right=�{�.  $��0���� $��Risorse/logo (1).png��$��Right=�$��198.507��$��59.702��$��Bottom=�$ 0.486,-0.09&�$��0,0,24.433,48.806q�$��3��r� -.    r�Name$ mediaElementObj��$
��Left=�$	��100��$��494,1095,0,-114.06q�$	��Top=�$	��100�� r� LoadedBehavior$ Manual=�