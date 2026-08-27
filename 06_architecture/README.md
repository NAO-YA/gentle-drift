# アーキテクチャ

## 初期無料版の構成

```text
Browser
├── LandingPage            # `Start`で既定のシーン設定付き鑑賞ページへ案内する
├── 操作UI                 # モチーフ・色合い・ゆらぎの設定と全画面表示の切替
├── VisualEngine + Canvas  # 10種類の自然モチーフの生成と描画
├── WindowsScreenSaverHost  # WPF + WebView2で`.scr`の起動・設定・プレビューを担う
└── localStorage           # ブラウザまたはWebView2内の設定保存
```

初期版は静的なウェブアプリとして公開し、バックエンド、データベース、認証、決済サービスには接続しない。描画と設定保存は利用者のブラウザ内で完結する。

## 技術選定

| レイヤー | 採用技術 | 役割 |
| --- | --- | --- |
| フロントエンド | HTML、CSS、JavaScript | 鑑賞画面と操作UIを提供する |
| 描画 | Canvas 2D | 10種類のゆらぎを薄黒の背景に描画する |
| 設定保存 | `localStorage` | 利用者が選んだ設定をブラウザ内に保存する |
| 公開 | Cloudflare Pages | 無料版の静的ファイルをHTTPSで配信する |
| Windowsホスト | WPF + WebView2 | 共有HTMLを`.scr`の全画面、設定、プレビューとして表示する |
| リリース | GitHub Actions + GitHub Releases | バージョンタグからWindows x64の配布ZIPを作成する |

## リポジトリ構成

```text
index.html                             # 完成版を開くための入口
_src/landing.html                      # 開発用ランディングページのソース
_src/index.html                        # 開発用の無料版ソース
_src/windows-screensaver/              # Windows用WPF/WebView2ホスト
04_output/index.html                   # 公開用ランディングページ
04_output/iyashi-no-yuragi/index.html  # 公開用の完成版
.github/assets/                        # GitHub README用の紹介画像
.github/workflows/release.yml          # Windows版リリースの自動ビルド
00_plan/                               # 機能仕様
06_architecture/                       # 初期版の構成と将来検討の記録
```

## 境界のルール

- `LandingPage`は`Start`用の既定URLクエリを組み立てて鑑賞ページへ遷移する。描画や設定の保存は行わない。
- 操作UIは設定を`VisualEngine`へ渡すほか、Fullscreen APIを通じて鑑賞ページ全体の全画面表示を切り替える。描画要素ごとの状態は管理しない。
- `VisualEngine`はモチーフの生成とゆらぎの更新、Canvasへの描画を担当する。
- `WindowsScreenSaverHost`は共有HTMLをローカルに同梱し、Windowsの`/s`、`/c`、`/p`起動形式をWebView2のウィンドウへ変換する。スクリーンセーバー中の終了入力とマルチモニター表示もここで扱う。
- 設定はブラウザの`localStorage`だけに保存し、外部へ送信しない。
- 初期版には`/api`、認証、決済、会員データを持たせない。

## 将来のPro版

Pro版は無料公開版の反応を確認してから設計する。アカウント、クラウド保存、決済が必要になった場合も、その時点で追加する体験に合わせて構成を決める。現時点でそれらのサービスやデータベースを導入しない。

詳細は [APIとイベント](./api-and-events.md)、[データモデル](./data-model.md)、[セキュリティとデプロイ](./security-and-deployment.md) を参照する。
