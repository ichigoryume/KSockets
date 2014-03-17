//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSATask.h"
#import "KSEvent.h"


@interface KSAcceptedEventArgs : NSObject

@property int           acceptedSocket;
@property NSString *    peerAddress;

@end



@interface KSAcceptTask : KSATask

@property KSEvent * accepted;
- (id)initWithPort:(uint)port loop:(BOOL)loop;

@end
