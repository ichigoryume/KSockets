//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "KSClientSocket.h"

@interface KSClientSocket ()

/*-------------------------------------------------------------------------------
 - for state
 -------------------------------------------------------------------------------*/
- (void)startConnect;
- (void)startReceive;
- (void)cleanup;
- (void)sendMessage:(NSString *)message;
- (void)dispatchReceivedMessages;
- (void)emitConnected;
- (void)emitDisconnected;
- (void)emitReceived:(NSString *)message;
- (void)emitError:(NSString *)message;

@end