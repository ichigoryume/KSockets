//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSEvent.h"

@interface KSClientSocket : NSObject

/*-------------------------------------------------------------------------------
 - public properties
 -------------------------------------------------------------------------------*/
@property KSEvent * connected;
@property KSEvent * disconnected;
@property KSEvent * received;   // ハンドラの引数はNSString型の受信メッセージ
@property KSEvent * error;      // ハンドラの引数はNSString型のエラーメッセージ


/*-------------------------------------------------------------------------------
 - public properties
 -------------------------------------------------------------------------------*/
@property NSString *  localAddress;     // read only
@property NSString *  serverAddress;    // read only
@property uint        serverPort;       // read only


/*-------------------------------------------------------------------------------
 - initializing methods
 -------------------------------------------------------------------------------*/
- (id)initVariableSizeSocket:(NSString *)serverAddress serverPort:(uint)port;
- (id)initFixedSizeSocket:(NSString *)serverAddress serverPort:(uint)port size:(int)size;
- (id)initTerminatedMessageSocket:(NSString *)serverAddress serverPort:(uint)port terminator:(NSString *)terminator;


/*-------------------------------------------------------------------------------
 - user requests
 -------------------------------------------------------------------------------*/
- (void)start;
- (void)close;
- (void)send:(NSString *)message;


@end
