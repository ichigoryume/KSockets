//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//

#import "KSIReceiver.h"

@interface KSTerminatedMessageReceiver : NSObject <KSIReceiver> 

- (id)initWithTerminator:(NSString *)terminator;

@end
