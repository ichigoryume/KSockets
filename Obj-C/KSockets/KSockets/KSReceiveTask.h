//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSATask.h"
#import "KSEvent.h"

@interface KSReceiveTask : KSATask

@property KSEvent * received;
@property KSEvent * socketClosed;

@property int       receivedSize;
@property char *    buffer;

- (id)initWithSocket:(int)socket;

@end
