つくよみちゃん対応版→[SAPI For COEIROINK](https://github.com/shigobu/SAPIForCOEIROINK)

# SAPI For VOICEVOX
Windows標準の音声合成機能の音声としてVOICEVOXが追加ができます。  
![コンパネ音声合成](https://user-images.githubusercontent.com/32050537/131860902-7aedbd45-453a-4425-a33c-7cb4a2f2dcf1.png)  
SAPI5対応のアプリケーションから使用することができます。  
現在、古い方の音声合成機能に対応しています。コントロールパネル→音声認識→音声合成 で表示される音声認識のプロパティ画面の音声の選択ドロップダウンに追加されます。  
新しい方の、設定アプリ→時刻と言語→音声認識→音声→音声を選択するドロップダウンには表示されません。  
したがって、新しい方の音声合成機能しか対応していないアプリには対応していません。  

32bit版は、棒読みちゃんでテストしています。  
64bit版は、やります！アンコちゃんでテストしています。  

## 使用方法
GitHubのReleaseからダウンロードができます。  
setup.exeを起動し、インストールしてください。  
32bit版と64bit版が存在します。使用したいアプリケーションに応じて選択してください。  
両方インストールすることもできます。その場合、同じ場所にインストールしないでください。  
インストール場所の初期値は「c:\SAPIForVOICEVOX(32or64)」になっています。  
「C:\Program Files (x86)」へインストールしないでください。何故か正常にインストールできません。  

VOICEVOX本体が必要です。[VOICEVOX公式ホームページ](https://voicevox.hiroshiba.jp/)  
VOICEVOXエンジンの自動起動機能は実装していないので、あらかじめVOICEVOXを起動しておいてください。  

Windows Defender にウイルスとして検知されてしまうと報告をうけています。  
当ソフトには、ウイルスは含まれていませんので、誤検知です。  
ソースコードを公開しているので、そこから判断していただくと幸いです。  
ちなみに、ウイルスバスターでは、「脅威無し」の判定でした。    

### アンインストール
アンインストールは、Windowsの「設定」→「アプリ」→「アプリと機能」からアンインストールができます。  
アンインストールを実行しても、設定を保存したxmlファイルが削除されずに残ります。  
不要な場合は、手動で削除をしてください。

### スタイル登録ツール
1.2.0から、スタイル登録ツールを追加しました。インストールの最後に自動で起動します。  
![スタイル登録ツール](https://user-images.githubusercontent.com/32050537/141030521-7442f17c-08b7-4b95-a4ff-2424c4212e9b.png)  
左のリストがVOICEVOXから取得したキャラクター情報です。右のリストがSAPIのリストです。  
「追加」ボタンを押すと、VOICEVOX側リストで選択されている項目がSAPI側リストへ追加されます。  
「削除」ボタンを押すと、SAPI側リストで選択されている項目が削除されます。  
「すべて追加」「すべて削除」は選択の有無に関わらず、すべて追加または削除されます。  
「OK」ボタンを押すことで、SAPI側リストの内容が登録されます。「OK」ボタンを押すまで登録は実行されません。  
キャラ・スタイル情報は、32bit版と64bit版でそれぞれ別に登録されます。  

設定画面同様に、スタートメニューの全てのアプリに追加されます。

### 設定画面
バージョン1.1.0から設定画面を追加しました。  
インストールすると、スタートメニューの全てのアプリに追加されます。  
![スクリーンショット 2021-11-19 212701](https://user-images.githubusercontent.com/32050537/142622829-65588d2e-6f7b-4506-922d-9ac3d85151b8.png)
![設定画面・調声](https://user-images.githubusercontent.com/32050537/133757251-033eb099-99e8-48f8-b4d7-f74580e1d142.png)  

この画面で、音声のパラメータ等を調節できます。  
設定内容は、32bit版と64bit版でそれぞれ別に保存されます。  

#### エンジンエラーの通知
読み上げる文によっては、エンジンエラーになる場合があります。  
読み上げ途中でエンジンエラーになった場合、その文は読み上げられずに代わりに「エンジンエラーです。」と音声が流れます。その後も文が続く場合で、正常に読み上げができた場合は読み上げが続きます。  
エラー音声を流したくない場合は、「エンジンエラーを通知する。」チェックボックスをオフにしてください。そうすることで、エンジンエラーをスキップすることができます。  

エンジンエラーを通知する例  
読み上げ文字列  
「こけこっこ。あっ。こけこっこ。」  
読み上げ音声  
「こけこっこ。エンジンエラーです。こけこっこ。」  

エンジンエラーを通知しない例  
読み上げ文字列  
「こけこっこ。あっ。こけこっこ。」  
読み上げ音声  
「こけこっこ。こけこっこ。」  

### N Airで使用するときの設定
**N Airで使用する場合、「SAPIイベントを使用する」のチェックをオフにして使用してください。**  
通常は、オンのままで問題無いはずです。

## 英語辞書について
バージョン1.1.1から英語辞書を搭載しました。  
辞書の大元は、  
The CMU Pronouncing Dictionary(http://www.speech.cs.cmu.edu/cgi-bin/cmudict)  
を使用しています。  
これを、「モリカトロン開発ブログ」の「英語をカタカナ表記に変換してみる(https://tech.morikatron.ai/entry/2020/05/25/100000)」  
という記事に載っているコードを使用し、発音記号からカタカナへ変換しています。

実際にどのように置換されるかは、[shigobu/EnglishKanaDictionary](https://github.com/shigobu/EnglishKanaDictionary)のテストアプリで確認するのが簡単かと思います。

## エラー音声
エラー音声には、VOICEVOXの「四国めたん」の音声を使用しています。  
エラー発生時には、予めVOICEVOXを使用し保存していたwavファイルを再生して、通知を行っています。

## ライセンス
このアプリのライセンスは、「MIT License」です。

また、VOICEVOX本体及び各音声ライブラリの利用規約にも従ってください。  
[VOICEVOX公式ホームページ](https://voicevox.hiroshiba.jp/)　(ダウンロード→利用規約から確認できます)  
[ずんだもん、四国めたん音源利用規約](https://zunko.jp/con_ongen_kiyaku.html)  
[春日部つむぎ利用規約](https://tsukushinyoki10.wixsite.com/ktsumugiofficial/%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84)  
[波音リツ利用規約](http://canon-voice.com/kiyaku.html)

## 開発環境
Microsoft Visual Studio Community 2019  
C#・C++

## ビルド方法
ソリューションを開き、ビルドを実行します。  
~~自動でCOMとして登録されます。SAPIに必要なレジストリの登録も自動で実行されます。~~  
インストーラプロジェクトを追加したので、ビルド時にレジストリ登録する機能はオフにしました。  
インストーラプロジェクトは自動でビルドされません。ソリューションエクスプローラーからSetupを右クリックしてビルドを選択し、ビルドしてください。  
同じく、Setupを右クリックしてインストールを選択すると、インストールができます。  

インストーラのビルドには、「Microsoft Visual Studio Installer Projects」の拡張機能が必要です。  
Visual Studioの「ツール」→「拡張機能」からインストールしてください。  

依存ライブラリは、NuGetで取得します。  
参照できない場合は、「NuGetパッケージの復元」を実行してください。  

## フォルダ構成
SAPIForVOICEVOX：本体  
SAPIGetStaticValueLib：必要な定数を取得するc++のライブラリ  
SFVvCommon：C#で使用する共通ライブラリ  
Setting：設定アプリ  
Setup：32bitインストーラ  
Setup64：64bitインストーラ  
SetupCustomActions：インストーラのカスタムアクション  
StyleRegistrationTool：スタイル登録ツール  

