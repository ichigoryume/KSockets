//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSATask.h"
#import "KSEvent.h"

@interface KSConnectTask : KSATask

@property KSEvent *     connected;
@property int           clientSocket;

- (id)initWithServerAddress:(NSString *)address port:(uint)port;

@end
