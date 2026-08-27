# データモデル

## 初期無料版

初期版は利用者アカウント、クラウドデータベース、決済データを持たない。設定は利用者のブラウザの`localStorage`にだけ保存する。

```ts
type SceneSettings = {
  scene: 'flame' | 'bamboo' | 'water' | 'cloud' | 'seaweed' | 'smoke' | 'sunlight' | 'petals' | 'fireflies' | 'curtain'
  palette: string
  amount: number
  sway: 'still' | 'gentle' | 'noticeable'
  seed: string
}
```

- このデータは利用者のブラウザにのみ存在する。
- サーバーへの同期、他の利用者との共有、個人情報との結び付けは行わない。
- ブラウザの保存データを削除すると、設定も削除される。

## 将来のPro版

Pro版でアカウントや保存機能を提供することを決めた場合にだけ、必要なデータモデルを設計する。現時点ではユーザー、会員、契約、紹介に関するテーブルを定義しない。
