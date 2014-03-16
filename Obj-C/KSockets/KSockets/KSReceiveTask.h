//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
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
