//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import <Foundation/Foundation.h>


// 受信データを一旦格納するバッファを管理するクラス。
// バッファにはMemoryStreamを使用する。バッファ容量は可変。
//
// Fixed, VariableSize などの各プロトコルに応じた受信処理から
// バッファ管理に関する処理を切り出しクラス化したもの。
// 各プロトコル向けに便利なヘルパー関数も提供する。
@interface KSReceiveBuffer : NSObject

// 現在の書き込み/読み込み位置/読み込み可能サイズ
@property (readonly) int readPosition;
@property (readonly) int writePosition;
@property (readonly, getter = length) int length;


// クリア。サイズも０に。
- (void)clear;


// バッファの末尾にdataを追加。
- (void)pushData:(void *)data length:(int)length;


// size分のデータをバッファから読み出す。
// コールするたびに length で得られる値が変わる（読み込んだ分読み込み開始位置をシークする）。
- (NSMutableData *)getData:(int)length NS_RETURNS_RETAINED;


// 現在の読み込み位置から4バイトのデータを int 型として読み込む。
// このメソッドのコール前後でLength()のコール結果は変わらない（読み込み開始位置をシークしない）。
- (int)readFirst4BytesAsInt;


// value で指定したものと同じbyte配列が、まだGetBytes()で返していない領域の何バイト目にあるか検索する。
// 見つからない場合は -1 を返す。
- (int)indexOf:(const char *)value length:(int)length;

@end
