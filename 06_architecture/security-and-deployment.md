# セキュリティとデプロイ

## 初期無料版の取り扱い

初期版はログイン、決済、サーバー側の利用者データ保存を行わない。設定は利用者自身のブラウザにだけ保存する。

- 画面上で個人情報や支払い情報を入力させない。
- 外部サービスの秘密情報やAPIキーをアプリに含めない。
- `localStorage`にはモチーフ、色合い、ゆらぎ、seedなど、鑑賞設定だけを保存する。

## デプロイ

```text
main（本番ブランチ）
  → 04_output/ の静的ファイルを確認
  → Cloudflare Pages の gentle-drift プロジェクトへ直接アップロード
  → https://gentle-drift.pages.dev
```

初回公開ではGitHub連携を使わず、Cloudflare Pagesへ直接デプロイする。公開コマンドは次のとおり。

```sh
wrangler pages deploy ./04_output --project-name gentle-drift --branch main
```

- Cloudflareへの認証はローカルの`wrangler login`によるOAuth認証を使う。APIトークン、アカウントID、メールアドレスなどの認証情報はリポジトリや配布ファイルに保存しない。
- 公開先はCloudflare PagesのHTTPSを利用する。
- 公開前に、外部APIや決済サービスへの接続が含まれていないことを確認する。
- 不具合報告や利用者の感想を受け取り、無料版の改善とPro版検討に活用する。

## GitHubとWindows版の配布

- ソースと紹介素材はGitHubの公開リポジトリ`NAO-YA/gentle-drift`で公開する。秘密情報、WebView2の利用者データ、Cloudflare認証情報は含めない。
- `v*`タグへのpushでGitHub ActionsがWindows x64版をビルドし、GitHub ReleaseのZIPとして公開する。
- リリースでは`.scr`と隣接する`Web`フォルダを一緒に配布する。HTMLはネットワークから読み込まず、ローカルファイルとしてWebView2に渡す。
- リポジトリは閲覧可能にするが、`LICENSE`でAll Rights Reservedを明記し、バイナリ・ソース・紹介素材の再配布や商用転用を許可しない。

## 将来のPro版

アカウント、決済、クラウド保存を採用する場合は、その時点で秘密情報の管理、認可、データ保護、運用手順を新たに設計する。初期版にはそのための設定や接続を追加しない。
