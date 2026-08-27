# Cloudflare Pages

## 公開先

- プロジェクト名: `gentle-drift`
- 本番ブランチ: `main`
- 公開URL: https://gentle-drift.pages.dev

## 公開手順

1. ローカルで`wrangler login`を実行し、CloudflareにOAuth認証する。
2. ワークスペースのルートで次を実行する。

```sh
wrangler pages deploy ./04_output --project-name gentle-drift --branch main
```

このプロジェクトはGitHub連携ではなく、`04_output`をCloudflare Pagesへ直接アップロードする方式で公開する。認証情報はソースコードや設定ファイルへ書き込まない。
